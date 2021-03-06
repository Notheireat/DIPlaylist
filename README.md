# DIPlaylist 
[![GitHub release](https://img.shields.io/github/release/Notheireat/DIPlaylist.svg)](https://github.com/Notheireat/DIPlaylist/releases/latest) [![AppVeyor](https://img.shields.io/appveyor/ci/coderz/diplaylist.svg)](https://ci.appveyor.com/project/coderz/diplaylist)

Программа создаёт плейлист интернет-радиостанции «Digitally Imported» для проигрывателя «AIMP» с активированной премиум-подпиской на 7 дней (формат вещания 320 kbit/sec MP3).

*Короткая ссылка: [`https://git.io/v7Hxq`](https://git.io/v7Hxq)*

## Параметры запуска
| Название | Описание | Пример |
| ----- | ----- | ---- |
| Путь сохранения | Свой путь сохранения плейлиста (с названием файла) | "D:\PLS\Digitally Imported.aimppl" |
| AIMP 3 | 3.x версия проигрывателя AIMP | -aimp3 |
| AIMP 4 | 4.x версия проигрывателя AIMP | -aimp4 |
| Тихий режим | Автоматическое создание плейлиста (без интерфейса) | -s |

Путь сохранения всегда должен стоять вначале. Параметры можно комбинировать.

## Планировщик заданий 
Программа имеет возможность запуска через планировщик заданий.

*Пример XML-файла для импорта: [`https://git.io/v7Hhr`](https://git.io/v7Hhr)*