<div class="header" align="center">
<img alt="New Horizons" width="880" height="300" src="https://raw.githubusercontent.com/ss14-new-horizons/ss14-new-horizons/master/Resourses/Textures/_NewHorizons/Logo/logo-nh.png">
</div>

Это репозиторий исходного кода проекта русскоязычного сервера **New Horizons** ("Новые Горизонты"), форка [Space Station 14](https://github.com/space-wizards/space-station-14), основанного на билде проекта [Corvax](https://github.com/space-syndicate/space-station-14).

**Space Station 14** — это ремейк SS13, который работает на собственном движке [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), написанном на C#.

**New Horizons** — это проект небольшого сообщества энтузиастов по игре Space Station 14, меняющий сеттинг игры на исследовательскую миссию в один конец на космическом корабле. На данный момент проект находится в стадии разработки и ставит целью поддерживать повышенный уровень ролевого отыгрыша благодаря нововведениям во внутриигровые инструменты и механики, фокусируясь на интересном повествовании, проработанности игровой вселенной, сюжете и атмосферности. Здесь каждый раунд становится интересной историей.

## Ссылки

[Наш Discord](https://discord.station14.ru) | [Наша Вики](https://station14.ru/) | [Steam](https://store.steampowered.com/app/1255460/Space_Station_14/) | [Клиент без Steam](https://spacestation14.io/about/nightlies/) | [Основной репозиторий](https://github.com/space-wizards/space-station-14)

## Документация

На официальном сайте с [документацией](https://docs.spacestation14.io/) имеется вся необходимая информация о контенте SS14, движке, дизайне игры и многом другом. Также имеется много информации для начинающих разработчиков.

Кроме того, ознакомьтесь со следующими ресурсами, содержащими информацию о лицензировании и указании авторства:
- [Robust Generic Attribution](https://docs.spacestation14.com/en/specifications/robust-generic-attribution.html)
- [Robust Station Image](https://docs.spacestation14.com/en/specifications/robust-station-image.html)

## Контрибьют

Мы рады принять вклад от любого человека. Заходите в Discord, если хотите помочь. У нас есть [список проблем](https://github.com/space-syndicate/space-station-14-content/issues), которые нужно решить, и любой может за них взяться. Не бойтесь просить о помощи!
Только убедитесь, что ваши изменения и PRы соответствуют [руководству по контрибьюту](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html).

## Политика к коду сгенерированным ИИ

Не принимаются материалы, созданные с помощью ИИ без должных усилий и проверки. К ним относятся, в частности:

- Любой код (включая YAML), сгенерированный через GitHub Copilot, ChatGPT и им подобными.
- Изображения, аудиофайлы и другие ресурсы, созданные с помощью ИИ.
- Автоматически сгенерированная документация, отчеты об ошибках (issue reports) или описания запросов на слияние.

## Сборка

1. Склонируйте этот репозиторий локально с помощью команды `git clone`
2. Запустите `RUN_THIS.py` для инициализации подмодулей и скачивания движка.
3. Скомпилируйте проект с помощью команды `dotnet build`.

[Более подробная инструкция по запуску проекта.](https://station14.ru/wiki/%D0%97%D0%B0%D0%BF%D1%83%D1%81%D0%BA_%D0%BB%D0%BE%D0%BA%D0%B0%D0%BB%D1%8C%D0%BD%D0%BE%D0%B3%D0%BE_%D1%81%D0%B5%D1%80%D0%B2%D0%B5%D1%80%D0%B0)

## Лицензия

Код этого репозитория (проекта) лицензирован и распространяется под лицензией [**GNU Affero General Public License версии 3.0 или более поздней (AGPL-3.0-or-later)**](https://github.com/ss14-new-horizons/ss14-new-horizons/blob/master/LICENSE-AGPL-3.0-or-later.TXT), если не указано иное.

Оригинальный код игры [Space Station 14](https://github.com/space-wizards/space-station-14) лицензирован и распространяется под лицензией [**MIT**](https://github.com/ss14-new-horizons/ss14-new-horizons/blob/master/LICENSE-MIT.TXT).

Этот репозиторий содержит код, источником которого являются иные авторы, репозитории или проекты. Условия лицензий такого кода должны соблюдаться вместе с условиями лицензии этого репозитория (проекта).

Некоторые файлы содержат заголовки-комментарии в соответствии со [спецификацией REUSE](https://reuse.software/) или отдельные/сопутствующие файлы (`license`) с информацией о лицензии, авторском праве и условиях повторного использования.

Ассеты имеют свою лицензию и авторские права в файле метаданных ([`meta.json`](https://docs.spacestation14.com/en/specifications/robust-station-image.html) или [`attributions.yml`](https://docs.spacestation14.com/en/specifications/robust-generic-attribution.html)). [Пример](https://github.com/ss14-new-horizons/ss14-new-horizons/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Большинство ассетов лицензированы под [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/), если не указано иное.

Обратите внимание, что некоторые ассеты лицензированы на некоммерческой основе [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) или аналогичной некоммерческой лицензией, и их необходимо удалить, если вы хотите использовать эти работы в коммерческих целях.

Владельцы проекта не претендуют на право собственности на работы (включая код, модификации, ассеты или ресурсы), созданные иными авторами, репозиториями, проектами, третьими сторонами или оригинальными разработчиками Space Station.
Все авторские права сохраняются за их законными правообладателями.
