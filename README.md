# 🎯 Cosmo Conquest

Цей проєкт поєднує сервер на PHP (гілка `server`) з клієнтом на Unity (гілка `main`) для реалізації багатокористувацької гри. Сервер обробляє запити до бази даних PostgreSQL, а Unity-клієнт взаємодіє з сервером через HTTP. Для локального розгортання використовується **Laragon**, **Ngrok**, **PostgreSQL**, та **Unity**.

---

## 📁 Структура репозиторію

```
- main/          # Unity client
- server/        # PHP server logic
```

---

## 🧰 Вимоги

- [Laragon](https://laragon.org/)
- [PostgreSQL](https://www.postgresql.org/)
- [Ngrok](https://ngrok.com/)
- [Unity Hub + Unity Editor](https://unity.com/)
- [Git](https://git-scm.com/)

---

## ⚙️ Налаштування серверної частини

### 1. Склонуй репозиторій і відкрий гілку `server`

```bash
git clone https://github.com/1vanytska/Cosmo-Conquest.git
cd Cosmo-Conquest
git checkout server
```

### 2. Налаштування Laragon

1. Відкрий Laragon.
2. У директорії `laragon/www/` створи папку проєкту:
   ```
   C:/laragon/www/game-server/
   ```
3. Скопіюй усі файли з гілки `server` до цієї папки.
4. У файлі `php.ini` переконайся, що `pgsql` розширення ввімкнено:
   ```ini
   extension=pgsql
   extension=pdo_pgsql
   ```

### 3. Налаштування PostgreSQL

1. Створи базу даних, наприклад `cosmo_game`.
2. Імпортуй SQL-структуру (наприклад, з `init.sql`, якщо є).
3. Відредагуй файл конфігурації (`db.php`) у `server/`:
   ```php
   $host = 'localhost';
   $port = '5432';
   $db = 'cosmo_game';
   $user = 'postgres';
   $pass = 'pass';
   ```

---

## 🌐 Налаштування Ngrok

1. Авторизуйся:
   ```bash
   ngrok config add-authtoken <your_token>
   ```

2. Запусти тунель до Laragon:
   ```bash
   ngrok http 80
   ```

3. Скопіюй виданий URL (наприклад, `https://abc123.ngrok.io`) і встав у Unity як `ServerConfig.BaseUrl`.

---

## 🧩 Налаштування клієнта Unity

### 1. Перейди на гілку `main`

```bash
git checkout main
```

### 2. Відкрий проєкт у Unity

1. Запусти **Unity Hub**.
2. Вибери **Add Project** → обери папку `main/`.

### 3. Задай серверну адресу

У скрипті `ServerConfig.cs`:
```csharp
public static class ServerConfig
{
    public static string BaseUrl = "https://abc123.ngrok.io"; // <- Твій ngrok URL
}
```

---

## ▶️ Запуск

1. Запусти **Laragon** (Apache + PostgreSQL).
2. Запусти **Ngrok**.
3. Запусти гру в Unity (Play Mode).
4. Тестуй взаємодію між гравцями.

---

## 🧪 Тестування

- Рекомендується запускати гру в **двох копіях Unity** або **на кількох пристроях**.
- Переконайся, що сервер відповідає (перевір через Postman або браузер).

---

## 🧹 Очищення даних

Сервер автоматично очищає дані після того, як **усі гравці переглянули результати гри**.

---

## 📦 Додатково

- Перевір, щоб всі запити у Unity використовували `UnityWebRequest` з `application/json`.
- Уникай CORS-проблем — працюючи через Ngrok, сервер сприймається як зовнішній.

---

## 💬 Питання / зворотній зв'язок

Для будь-яких питань або пропозицій — створіть `issue` або напишіть мені на [sofiia.ivanytska.22@pnu.edu.ua].
