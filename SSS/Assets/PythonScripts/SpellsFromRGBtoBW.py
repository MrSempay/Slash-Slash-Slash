import os
import cv2
from PIL import Image, ImageOps

# Настройки
input_folder1 = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\Assets\\Resources\\Images\\Spells"  # берём отсюда
input_folder2 = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\Assets\\Resources\\Images\\Spells\\SpellsFirstFrame"  # и отсюда
output_folder = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\Assets\\Resources\\Images\\Spells\\SpellsBW"     # ЧБ кладём сюда
inputPathes = [input_folder1, input_folder2]
# Создаём папку для ч/б изображений
os.makedirs(output_folder, exist_ok=True)

# Поддерживаемые форматы
valid_exts = ('.jpg', '.jpeg', '.png', '.bmp', '.tiff')

for input_folder in inputPathes:
    for filename in os.listdir(input_folder):
        if filename.lower().endswith(valid_exts):
            try:
            
                name, ext = os.path.splitext(filename)
                # Формируем новое имя
                output_filename = f"{name}Disabled{ext}"

                output_path = os.path.join(output_folder, f"{output_filename}")
                
                if os.path.exists(output_path):
                    print(f"Файл уже существует, пропускаем: {output_filename}")
                    continue
                
                # Открываем изображение с PIL
                img = Image.open(os.path.join(input_folder, filename))        
                

                # Конвертируем в ч/б с сохранением прозрачности
                if img.mode == 'RGBA':
                    bw = ImageOps.grayscale(img)
                    bw.putalpha(img.split()[3]) # Копируем альфа-канал
                else:
                    bw = ImageOps.grayscale(img)


                # Сохраняем
                
                bw.save(output_path)

                print(f"Обработано: {filename}")
            except Exception as e:
                print(f"Ошибка с {filename}: {str(e)}")



# for filename in os.listdir(input_folder1):
    # if filename.lower().endswith(valid_exts):
        # try:
        
            # name, ext = os.path.splitext(filename)
            # # Формируем новое имя
            # output_filename = f"bw_{name}Disabled{ext}"

            # output_path = os.path.join(output_folder, f"bw_{output_filename}")
            
            # if os.path.exists(output_path):
                # print(f"Файл уже существует, пропускаем: {output_filename}")
                # continue
            
            # # Открываем изображение с PIL
            # img = Image.open(os.path.join(input_folder1, filename))        
            

            # # Конвертируем в ч/б с сохранением прозрачности
            # if img.mode == 'RGBA':
                # bw = ImageOps.grayscale(img)
                # bw.putalpha(img.split()[3]) # Копируем альфа-канал
            # else:
                # bw = ImageOps.grayscale(img)


            # # Сохраняем
            
            # bw.save(output_path)

            # print(f"Обработано: {filename}")
        # except Exception as e:
            # print(f"Ошибка с {filename}: {str(e)}")
            
            
# for filename in os.listdir(input_folder2):
    # if filename.lower().endswith(valid_exts):
        # try:
        
            # name, ext = os.path.splitext(filename)
            # # Формируем новое имя
            # output_filename = f"bw_{name}Disabled{ext}"
           
            # output_path = os.path.join(output_folder, f"bw_{output_filename}")
            
            # if os.path.exists(output_path):
                # print(f"Файл уже существует, пропускаем: {output_filename}")
                # continue
            
            # # Открываем изображение с PIL
            # img = Image.open(os.path.join(input_folder2, filename))        
            

            # # Конвертируем в ч/б с сохранением прозрачности
            # if img.mode == 'RGBA':
                # bw = ImageOps.grayscale(img)
                # bw.putalpha(img.split()[3]) # Копируем альфа-канал
            # else:
                # bw = ImageOps.grayscale(img)


            # # Сохраняем
            # bw.save(output_path)

            # print(f"Обработано: {filename}")
        # except Exception as e:
            # print(f"Ошибка с {filename}: {str(e)}")

print("Готово! Ч/б копии сохранены в:", output_folder)


os.system("pause") # требует нажатие любой клавиши