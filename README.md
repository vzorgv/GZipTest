# GZipTest
Разработать консольное приложение на C# для поблочного сжатия и распаковки файлов с помощью System.IO.Compression.GzipStream.

Для сжатия исходный файл делится на блоки одинакового размера, например, в 1 мегабайт. Каждый блок сжимается и записывается в выходной файл независимо от остальных блоков.

Программа должна эффективно распараллеливать и синхронизировать обработку блоков в многопроцессорной среде и уметь обрабатывать файлы, размер которых превышает объем доступной оперативной памяти. 

В случае исключительных ситуаций необходимо проинформировать пользователя понятным сообщением, позволяющим пользователю исправить возникшую проблему, в частности, если проблемы связаны с ограничениями операционной системы.
При работе с потоками допускается использовать только базовые классы и объекты синхронизации (Thread, Manual/AutoResetEvent, Monitor, Semaphor, Mutex) и не допускается использовать async/await, ThreadPool, BackgroundWorker, TPL.
Код программы должен соответствовать принципам ООП и ООД (читаемость, разбиение на классы и т.д.). 

Параметры программы, имена исходного и результирующего файлов должны задаваться в командной строке следующим образом:
GZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла]
В случае успеха программа должна возвращать 0, при ошибке возвращать 1.

Примечание: формат архива остаётся на усмотрение автора, и не имеет значения для оценки качества тестового, в частности соответствие формату GZIP опционально.

## Допущения
* Драйвер устройства позволяет произвольное перемещение по файлу и параллельную чтение\запись
* Допускается реализация базовых требований и не являющаяся production ready решением 

## Результаты тестирования
Intel Core i7, 4 cores, 8Gb RAM, SSD, Win 10

Размер исходного файла для сжатия: 17Gb
Размер блока при сжатии 1Mb
Размер сжатого файла 103Mb

Среднее время сжатия:
1 поток - 17секунд
8 потоков - 8 секунд

Среднее время распаковки:
1 поток - 26 секунд
8 потоков - 30 секунд (есть возможность улучшения, но для тестового задания не реализовано)

Потребляемая память в среднем 21Мb

## Реализация

C#, .Net Core 3.1 без использования дополнительных NuGet  зависимостей

Unit tests с учетом допущений реализованы только для нескольких сценариев.

Документирование кода с учетом допущений минимальное.

Для production ready необходим небольшой рефакторинг в части функциональной декомпозиции классов CompressTask и DecompressTask. Более полное покрытие тестами в т.ч. и бенчмарками