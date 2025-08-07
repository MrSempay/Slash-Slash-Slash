import os
import shutil
source_folder = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\Assets\\Resources\\Images\\Ammunition"  # Замените на реальный путь
destination_folder = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\Assets\\Resources\\Images\\Ammunition\\AmmunitionDisabled"  # Папка будет создана, если её нет

# Поддерживаемые форматы изображений
image_extensions = ('.jpg', '.jpeg', '.png', '.bmp', '.tiff', '.gif', '.webp')

# Создаём целевую папку, если не существует
os.makedirs(destination_folder, exist_ok=True)

# Счётчик скопированных файлов
copied_files = 0

for filename in os.listdir(source_folder):
    # Проверяем расширение файла
    if filename.lower().endswith(image_extensions):
        try:
            # Формируем полные пути
            src_path = os.path.join(source_folder, filename)
            dst_path = os.path.join(destination_folder, filename)
            
            # Копируем файл
            shutil.copy2(src_path, dst_path)  # copy2 сохраняет метаданные
            copied_files += 1
            print(f"Скопировано: {filename}")
        except Exception as e:
            print(f"Ошибка при копировании {filename}: {str(e)}")

print(f"\nГотово! Скопировано файлов: {copied_files}")
print(f"Путь к копиям: {destination_folder}")