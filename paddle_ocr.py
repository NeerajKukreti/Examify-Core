# coding: utf-8
import sys
from paddleocr import PaddleOCR

# Force Python output to UTF-8
sys.stdout.reconfigure(encoding='utf-8')

# Best config for English + Hindi MCQs
ocr = PaddleOCR(
    lang='ch',           # uses the Chinese + multilingual model
    use_angle_cls=True,
    use_gpu=False
)


if __name__ == '__main__':
    if len(sys.argv) < 2:
        print("Usage: python paddle_ocr.py <image_path>")
        sys.exit(1)
    
    image_path = sys.argv[1]
    result = ocr.ocr(image_path, cls=True)

    lines = []
    for block in result:
        for line in block:
            bbox = line[0]
            text = line[1][0]
            conf = line[1][1]
            top_y = bbox[0][1]
            lines.append((top_y, text, conf))

    # Sort top-to-bottom
    lines.sort(key=lambda x: x[0])

    for _, text, conf in lines:
        # safe unicode print
        print(f"{conf:.3f} | {text}")
