# Hissa qo'shish / Contributing

Xato yoki taklif uchun Issue oching. O'zgarish uchun alohida branch va Pull Request yarating. Issues and pull requests in English are welcome too.

## Qoidalar

- Mantiq `TaqvimWidget.Core` ga yoziladi va testlanadi. `App` faqat chizish va Windows bilan ishlash uchun.
- Yangi tashqi kutubxona qo'shishdan oldin Issue'da muhokama qiling.
- Taymerdagi har bir yangi ish resurs sarfini oshiradi: davrini va nega kerakligini tushuntiring.
- Kod uslubi `.editorconfig` da. Ogohlantirishlar yig'ishni to'xtatadi.
- Foydalanuvchiga ko'rinadigan matnlar o'zbek tilida.
- Skrinshot yoki log yuborishdan oldin shaxsiy ma'lumotlarni olib tashlang.

PR yuborishdan oldin:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

## Reliz chiqarish

`CHANGELOG.md` ni yangilang, keyin `v2.1.0` ko'rinishidagi teg yuboring. GitHub Actions o'rnatuvchini yig'ib, relizga joylaydi.
