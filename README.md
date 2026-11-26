# Taqvim Widget

[![CI](../../actions/workflows/ci.yml/badge.svg)](../../actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**O'zbekcha** · [English](README.en.md)

Windows 10/11 uchun yengil widget: internet tezligi soat yonida, namoz vaqtlari, kalendar va eslatmalar bitta oynada.

<img src="docs/images/widget.png" alt="Taqvim Widget oynasi (namunaviy ma'lumotlar bilan)" width="340">

## Imkoniyatlar

- **Internet tezligi:** vazifalar panelida soat yonida, 60 soniyalik grafik, kunlik va oylik trafik, oylik limit.
- **Aloqa nazorati:** ping ko'rsatkichi, internet uzilsa va tiklansa bildirishnoma.
- **Tezlik testi:** faqat tugma bosilganda ishlaydi.
- **Namoz vaqtlari:** 13 shahar, internetsiz hisoblanadi, hozirgi va keyingi vaqt ajratib ko'rsatiladi, oldindan eslatma.
- **Kalendar:** ISO hafta raqamlari, bayramlar, kunlarga eslatma va vaqtli bildirishnoma.
- **Tizim:** CPU, RAM, batareya.
- **Pomodoro** taymeri.
- Windows 11 uslubi, yorug' va qorong'i mavzu, aksent rangi, DPI masshtabi.

## O'rnatish

1. [Releases](../../releases/latest) sahifasidan `TaqvimWidget-Setup.exe` faylini yuklab oling.
2. Faylni ishga tushiring va **O'rnatish** tugmasini bosing. Administrator huquqi kerak emas.

Fayl raqamli imzolanmagan, shuning uchun Windows SmartScreen ogohlantirishi chiqishi mumkin. Faylning SHA-256 yig'indisi har bir relizga ilova qilinadi. Xohlasangiz, dasturni [manba koddan o'zingiz yig'ing](#yigish).

Talab: Windows 10 (1903+) yoki Windows 11. Qo'shimcha dastur o'rnatish shart emas (.NET Framework 4.8 Windows tarkibida bor).

O'chirish: **Sozlamalar → Ilovalar → Taqvim Widget**.

## Ishlatish

| Amal | Natija |
|---|---|
| Soat yonidagi tezlik yozuvi yoki tray ikonkasini bosish | Widget ochiladi / yopiladi |
| O'ng tugma | Sozlamalar, chiqish |
| Kalendardagi kunni bosish | Shu kunning eslatmalari |
| Sichqoncha g'ildiragi (widget ustida) | Oyni almashtirish |

## Resurs sarfi

Bitta noutbukda, widget yopiq holatda 60 soniya davomida o'lchangan:

| Ko'rsatkich | Qiymat |
|---|---|
| CPU (bitta yadroga nisbatan) | ~0.1% |
| Private bytes | ~27 MB |
| Private working set | ~9 MB |
| O'rnatuvchi hajmi | ~150 KB |

Qiymatlar kompyuterga qarab farq qiladi. Bunga qanday erishilgani [arxitektura hujjatida](docs/ARCHITECTURE.md#resurs-sarfi) yozilgan.

## Maxfiylik

Telemetriya, reklama va hisob yo'q. Ma'lumotlar faqat kompyuteringizda, `%APPDATA%\TaqvimWidget` papkasida saqlanadi.

Dastur tarmoqqa faqat quyidagi hollarda murojaat qiladi:

| Qachon | Qayerga | Nima uchun |
|---|---|---|
| Har 8 soniyada | `1.1.1.1`, `8.8.8.8` | ICMP ping (aloqa holati) |
| Faqat tezlik testi tugmasi bosilganda | `speed.cloudflare.com` | Tezlik o'lchash. Har bir testda o'nlab MB trafik sarflanishi mumkin |

Tezlik ko'rsatkichi tarmoq adapterining hisoblagichlaridan o'qiladi va trafik sarflamaydi.

## Namoz vaqtlari haqida

Vaqtlar dastur ichida, [islom.uz](https://islom.uz) ishlatadigan parametrlarga moslab hisoblanadi. Bu loyiha islom.uz yoki biror diniy idoraning rasmiy mahsuloti **emas**. Muhim hollarda rasmiy taqvimni tekshiring. Batafsil: [docs/PRAYER_TIMES.md](docs/PRAYER_TIMES.md).

## Yigish

[.NET SDK 8.0+](https://dotnet.microsoft.com/download) kerak.

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

Natija: `dist\TaqvimWidget-Setup.exe`. Faqat testlar:

```powershell
dotnet test tests\TaqvimWidget.Core.Tests
```

## Hujjatlar

- [Arxitektura](docs/ARCHITECTURE.md)
- [Namoz vaqtlari hisobi](docs/PRAYER_TIMES.md)
- [Hissa qo'shish](CONTRIBUTING.md)
- [O'zgarishlar tarixi](CHANGELOG.md)

## Litsenziya

[MIT](LICENSE)
