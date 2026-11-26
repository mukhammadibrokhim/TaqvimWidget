# Arxitektura

## Loyihalar

```
src/
  TaqvimWidget.Core    Mantiq. UI va Windows API'ga bog'liq emas, testlar bilan qoplangan
  TaqvimWidget.App     WinForms ilovasi: interfeys va Windows integratsiyasi
  TaqvimWidget.Setup   O'rnatuvchi (dastur fayllari resurs sifatida ichida)
  Shared               App va Setup uchun umumiy kod (nomlar, yo'llar, yorliqlar)
tests/
  TaqvimWidget.Core.Tests
```

Bog'liqlik bir tomonga: `App → Core`. `Setup` dasturga kod sifatida bog'lanmaydi, faqat yig'ilgan fayllarni joylaydi.

## Nega .NET Framework 4.8

| Variant | Foydalanuvchida kerak | Natija |
|---|---|---|
| **.NET Framework 4.8** | Hech narsa (Windows tarkibida) | Bir necha yuz KB, tez ishga tushadi |
| .NET (zamonaviy), framework-dependent | Desktop Runtime o'rnatish | Kichik fayl, lekin qo'shimcha qadam |
| .NET (zamonaviy), self-contained | Hech narsa | Hajm o'nlab MB |

Kompilyatsiya zamonaviy SDK bilan bo'ladi (C# 10, nullable reference types, ogohlantirishlar xato hisoblanadi). Tashqi runtime kutubxonalari yo'q.

## Ish jarayoni

`WidgetContext` bitta 1 soniyalik UI taymerini boshqaradi:

| Davr | Vazifa |
|---|---|
| 1 s | Adapter hisoblagichlari → `SpeedMeter` → `TrafficLedger`, pomodoro, taskbar yozuvi |
| 1 s, faqat oyna ochiq | CPU/RAM/batareya, qayta chizish |
| 8 s | Ping (asinxron) |
| 15 s | Eslatma va namoz bildirishnomalari |
| 60 s | Trafikni saqlash (faqat o'zgargan bo'lsa) |

Barcha holat UI oqimida o'zgaradi. Tezlik testi fon oqimida ishlaydi va natija `await` orqali UI oqimiga qaytadi, shuning uchun qulflar kerak emas.

## Resurs sarfi

- Tarmoq adapterlari ro'yxati (qimmat so'rov) faqat `NetworkAddressChanged` hodisasida va zaxira sifatida daqiqada bir yangilanadi.
- Taskbar yozuvi matni, holati yoki o'rni o'zgargandagina qayta chiziladi.
- Cho'tka, qalam va shriftlar `Theme` ichida bir marta yaratiladi. Grafik nuqtalari massivi qayta ishlatiladi, tezlik tarixi halqasimon buferda.
- Namoz vaqtlari kunlik keshlanadi.
- Yopiq oynada tizim ko'rsatkichlari so'ralmaydi.
- Tezlik testi ma'lumoti 64 KB bufer orqali oqim sifatida o'tadi.
- Jarayon ustuvorligi `BelowNormal`.

## Windows integratsiyasi

- **Taskbar yozuvi:** fokus olmaydigan (`WS_EX_NOACTIVATE`), eng yuqorida turadigan shaffof oyna. U `TrayNotifyWnd` chap tomoniga joylanadi. Bu Windows'ning rasmiy API'si emas, shuning uchun kelajakdagi yangilanishlar ta'sir qilishi mumkin. Sozlamalardan o'chirsa bo'ladi.
- **Bildirishnomalar:** WinRT API COM interop orqali chaqiriladi (`Interop/WinRtToast.cs`). Windows xabarni ko'rsatishi uchun Start menyudagi yorliqda `AppUserModelID` bo'lishi **shart**. Uni o'rnatuvchi yozadi, o'rnatilgan dastur esa har ishga tushganda tiklaydi. WinRT ishlamasa, tray balloni ishlatiladi.

## Ma'lumotlar

`%APPDATA%\TaqvimWidget`:

| Fayl | Tarkib |
|---|---|
| `config.json` | Sozlamalar. Chegaradan tashqari qiymatlar yuklashda to'g'rilanadi |
| `traffic.json` | `{"yyyy-MM-dd": [qabul, yuborish]}`, 120 kun saqlanadi |
| `notes.json` | Eslatmalar |
| `error.log` | Xatolar (256 KB dan oshsa tozalanadi) |

Fayllar atomik yoziladi (vaqtinchalik fayl → almashtirish). Buzilgan fayl dasturni to'xtatmaydi. 1.x versiya ma'lumotlari birinchi ishga tushishda nusxalanadi.

## Ishlab chiqish

Widgetni namunaviy ma'lumotlar bilan rasmga chizish (shaxsiy ma'lumotlar o'qilmaydi):

```powershell
.\src\TaqvimWidget.App\bin\Release\net48\TaqvimWidget.exe /snapshot "$PWD\docs\images\widget.png"
```

Buyruq qatori: `/show` (oyna ochiq holda ishga tushirish), `/uninstall`. O'rnatuvchi: `/silent`.
