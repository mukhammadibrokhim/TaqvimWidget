# Namoz vaqtlari hisobi

Kod: [`PrayerCalculator.cs`](../src/TaqvimWidget.Core/Prayer/PrayerCalculator.cs). Algoritm quyoshning holatini astronomik formulalar bilan hisoblaydi (praytimes.org yondashuvi) va internet talab qilmaydi.

## Standart parametrlar

| Vaqt | Qoida |
|---|---|
| Bomdod | Quyosh ufqdan 15.5° pastda |
| Quyosh | Chiqish balandligi 0.833° (+15 soniya tuzatish) |
| Peshin | Quyosh zenitdan o'tishi (tush) |
| Asr | Hanafiy: soya uzunligi 2 barobar (Shofe'iy: 1 barobar) |
| Shom | Quyosh botishi + 4 daqiqa |
| Xufton | Quyosh ufqdan 15.5° pastda |

Vaqt mintaqasi UTC+5. Natija eng yaqin daqiqaga yaxlitlanadi.

Parametrlar [islom.uz](https://islom.uz) sahifasidagi hisobga moslab tanlangan. Sayt ularni o'zgartirishi mumkin. **Bu loyiha islom.uz yoki biror diniy idora bilan bog'liq emas** va rasmiy taqvim o'rnini bosmaydi.

Burchaklar, Asr usuli va shahar sozlamalardan o'zgartiriladi.

## Testlar

`PrayerTests.cs` bir nechta sana va shahar uchun etalon qiymatlarni tekshiradi. Etalonlar loyihaning oldingi versiyasidan olingan: ular hisob o'zgarib qolmaganini kafolatlaydi, lekin mustaqil rasmiy manba hisoblanmaydi.

## Bayramlar

Qat'iy sanali bayramlar kodda berilgan. Ramazon va Qurbon hayiti Umm al-Qura taqvimidan olinadi, shuning uchun **taxminiy** (`*` belgisi bilan). Rasmiy sana bir kunga farq qilishi mumkin.
