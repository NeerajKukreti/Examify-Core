import sys
import io
import warnings
import json
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
warnings.filterwarnings('ignore')

import easyocr

reader = easyocr.Reader(['en', 'hi'], verbose=False)

if __name__ == '__main__':
    if len(sys.argv) < 2:
        sys.exit(1)
    
    image_path = sys.argv[1]
    result = reader.readtext(image_path, detail=1)
    
    if result:
        for line in result:
            text = line[1]
            print(text.encode('utf-8', errors='replace').decode('utf-8'))
