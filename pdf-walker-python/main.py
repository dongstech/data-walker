import tesserocr
# from PIL import Image

print(tesserocr.tesseract_version())  # print tesseract-ocr version
print(tesserocr.get_languages())  # prints tessdata path and list of available languages

imageRoot='/Users/fqdong/Downloads/pdfwalker_working/data/input'
imagePath = f'{imageRoot}/test1.png'
charLang = 'chi_sim'
# image = Image.open(imagePath)
# print(tesserocr.image_to_text(image))  # print ocr text from image
# or
print(tesserocr.file_to_text(imagePath, charLang))