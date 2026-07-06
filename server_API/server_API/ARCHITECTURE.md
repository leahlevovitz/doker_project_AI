# Monolith Architecture — Chinese Auction API

## Diagram

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | /api/auth/register | Register user |
| POST | /api/auth/login | Login, get JWT |
| GET/POST/PUT/DELETE | /api/donors | Manage donors |
| GET/POST/PUT/DELETE | /api/gifts | Manage gifts [manager] |
| GET/POST/DELETE | /api/purchasers | Purchases & basket [client] |
| POST | /api/lottery/draw/{giftId} | Run lottery |
| GET | /api/lottery/{giftId}/winners | Get winners |
| GET | /api/lottery/report | Full report |

## 3 Problems at Scale

1. **Single point of failure** — כל הלוגיקה רצה בתהליך אחד. עומס בהגרלות חוסם גלישה במתנות. אי אפשר לסקייל רק חלק מהמערכת.

2. **Shared database bottleneck** — כל הדומיינים חולקים DB אחד. שאילתה איטית בהגרלה נועלת טבלאות שה-purchase flow צריך. שינוי schema דורש deploy של כל המערכת.

3. **Deploy risk** — באג במודול אחד מוריד את כל המערכת. אין isolation בין components, exception ב-Lottery יכול לקרוס את Auth ו-Gifts יחד איתו.
