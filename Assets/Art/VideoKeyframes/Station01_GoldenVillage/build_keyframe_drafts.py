from pathlib import Path

from PIL import Image, ImageDraw, ImageEnhance, ImageOps


ROOT = Path(__file__).resolve().parent
MASTER = ROOT / "K01_master_locked.png"
STATION = ROOT / "station_building_cutout.png"
WOMAN = ROOT / "waving_woman_cutout.png"


def resize_about_point(image: Image.Image, scale: float, center: tuple[int, int]) -> Image.Image:
    width, height = image.size
    scaled = image.resize(
        (round(width * scale), round(height * scale)),
        Image.Resampling.LANCZOS,
    )
    center_x, center_y = center
    left = round(center_x * scale - center_x)
    top = round(center_y * scale - center_y)
    return scaled.crop((left, top, left + width, top + height))


def vertical_mask(size: tuple[int, int], top: int, bottom: int, max_alpha: int) -> Image.Image:
    width, height = size
    mask = Image.new("L", size, 0)
    draw = ImageDraw.Draw(mask)
    for y in range(top, height):
        alpha = max_alpha if y >= bottom else round(max_alpha * (y - top) / (bottom - top))
        draw.line((0, y, width, y), fill=alpha)
    return mask


def crop_alpha(image: Image.Image) -> Image.Image:
    bounds = image.getchannel("A").getbbox()
    return image.crop(bounds) if bounds else image


def advanced_scene(master: Image.Image, middle_scale: float, foreground_scale: float) -> Image.Image:
    width, height = master.size
    vanish = (round(width * 0.503), round(height * 0.448))

    middle = resize_about_point(master, middle_scale, vanish)
    middle_mask = vertical_mask(master.size, round(height * 0.34), round(height * 0.54), 112)
    result = Image.composite(middle, master, middle_mask)

    foreground = resize_about_point(master, foreground_scale, vanish)
    foreground_mask = vertical_mask(master.size, round(height * 0.43), round(height * 0.68), 255)
    return Image.composite(foreground, result, foreground_mask).convert("RGBA")


def track_right_x(width: int, height: int, y: int) -> int:
    vanish_x = round(width * 0.503)
    vanish_y = round(height * 0.448)
    bottom_x = round(width * 0.548)
    ratio = max(0.0, min(1.0, (y - vanish_y) / (height - vanish_y)))
    return round(vanish_x + (bottom_x - vanish_x) * ratio)


def add_platform(image: Image.Image, start_y: int, end_y: int, width_at_end: int) -> None:
    width, height = image.size
    vanish_x = round(width * 0.503)
    vanish_y = round(height * 0.448)
    end_y = min(end_y, height)
    start_y = max(start_y, vanish_y + 2)
    inner_start = track_right_x(width, height, start_y) + 10
    inner_end = track_right_x(width, height, end_y) + 30
    outer_start = inner_start + 7
    outer_end = inner_end + width_at_end

    polygon = [(inner_start, start_y), (outer_start, start_y), (outer_end, end_y), (inner_end, end_y)]

    mask = Image.new("L", image.size, 0)
    ImageDraw.Draw(mask).polygon(polygon, fill=242)
    noise = Image.effect_noise(image.size, 20)
    texture = ImageOps.colorize(noise, black=(108, 101, 89), white=(178, 164, 139)).convert("RGBA")
    texture.putalpha(mask)
    image.alpha_composite(texture)

    layer = Image.new("RGBA", image.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(layer)
    draw.line((inner_start, start_y, inner_end, end_y), fill=(200, 187, 164, 245), width=3)
    draw.line((outer_start, start_y, outer_end, end_y), fill=(96, 90, 80, 155), width=2)

    for step in range(1, 12):
        ratio = step / 12
        y = round(start_y + (end_y - start_y) * ratio)
        inner = round(inner_start + (inner_end - inner_start) * ratio)
        outer = round(outer_start + (outer_end - outer_start) * ratio)
        draw.line((inner, y, outer, y), fill=(188, 174, 150, 54), width=1)

    image.alpha_composite(layer)


def add_asset(
    image: Image.Image,
    asset_path: Path,
    target_width: int,
    x: int,
    baseline_y: int,
    saturation: float = 0.82,
    brightness: float = 1.03,
) -> None:
    asset = crop_alpha(Image.open(asset_path).convert("RGBA"))
    target_height = max(1, round(asset.height * target_width / asset.width))
    asset = asset.resize((target_width, target_height), Image.Resampling.LANCZOS)
    asset = ImageEnhance.Color(asset).enhance(saturation)
    asset = ImageEnhance.Brightness(asset).enhance(brightness)
    image.alpha_composite(asset, (x, baseline_y - target_height))


def build_frames(master: Image.Image) -> list[Image.Image]:
    frames: list[Image.Image] = [master.convert("RGBA")]

    # middle_scale, foreground_scale, station_width, station_x, station_baseline
    configs = [
        (1.012, 1.075, 22, 906, 428),
        (1.025, 1.145, 48, 944, 448),
        (1.040, 1.225, 95, 1008, 494),
        (1.058, 1.325, 190, 1138, 594),
        (1.078, 1.445, 310, 1350, 714),
        (1.095, 1.565, 390, 1580, 820),
        (1.112, 1.690, 0, 0, 0),
    ]

    for index, (middle_scale, foreground_scale, station_width, station_x, station_baseline) in enumerate(configs, 2):
        frame = advanced_scene(master, middle_scale, foreground_scale)
        if index == 4:
            add_platform(frame, 431, 590, 82)
        elif index == 5:
            add_platform(frame, 431, 940, 285)
        elif index == 6:
            add_platform(frame, 438, 940, 390)
        elif index == 7:
            add_platform(frame, 590, 940, 245)

        if station_width:
            add_asset(frame, STATION, station_width, station_x, station_baseline)

        if index == 6:
            add_asset(frame, WOMAN, 31, 1235, 739, saturation=0.88, brightness=1.06)

        frames.append(frame)

    return frames


def build_contact_sheet(frames: list[Image.Image]) -> Image.Image:
    thumb_width, thumb_height = 640, 360
    margin, label_height = 18, 28
    sheet = Image.new("RGB", (thumb_width * 2 + margin * 3, (thumb_height + label_height) * 4 + margin * 5), (32, 32, 32))
    draw = ImageDraw.Draw(sheet)
    for index, frame in enumerate(frames):
        x = margin + (index % 2) * (thumb_width + margin)
        y = margin + (index // 2) * (thumb_height + label_height + margin)
        thumb = frame.convert("RGB").resize((thumb_width, thumb_height), Image.Resampling.LANCZOS)
        sheet.paste(thumb, (x, y + label_height))
        draw.text((x, y + 4), f"K{index + 1:02d}", fill=(240, 240, 240))
    return sheet


def build_overlay(master: Image.Image, frame: Image.Image) -> Image.Image:
    overlay = Image.blend(master.convert("RGB"), frame.convert("RGB"), 0.5)
    draw = ImageDraw.Draw(overlay)
    width, height = overlay.size
    vanish = (round(width * 0.503), round(height * 0.448))
    draw.line((vanish[0] - 18, vanish[1], vanish[0] + 18, vanish[1]), fill=(220, 32, 32), width=2)
    draw.line((vanish[0], vanish[1] - 18, vanish[0], vanish[1] + 18), fill=(220, 32, 32), width=2)
    draw.text((16, 16), "K01 / K02 overlay - red cross: locked vanishing point", fill=(220, 32, 32))
    return overlay


def main() -> None:
    master = Image.open(MASTER).convert("RGB")
    frames = build_frames(master)
    for index, frame in enumerate(frames, 1):
        frame.convert("RGB").save(ROOT / f"K{index:02d}_formal.png", quality=95)
    build_contact_sheet(frames).save(ROOT / "K01-K08_contact_sheet.png", quality=95)
    build_overlay(master, frames[1]).save(ROOT / "K01_K02_formal_overlay.png", quality=95)


if __name__ == "__main__":
    main()
