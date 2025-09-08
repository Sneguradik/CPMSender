# Market Index Price Collector

Приложение, которое собирает текущие рыночные цены и отправляет их во внешний контур для создания индексов.
Сервис рассчитан на контейнерный запуск и не требует внешних зависимостей кроме настроек, передаваемых на этапе сборки образа.

## 0. Подготовка
Склонируем проект:
```bash
git clone https://gitlab.corp.spbe/v.ivanov/cpmsender.git
cd cpmsender
```

---

## 1. Сборка 
Сборка производится в докере

- На Unix
  ```bash
  docker build -f CPMSender/Dockerfile -t cpmsender .
    ```
- На Widows
    ```bash
  docker build -f CPMSender\Dockerfile -t cpmsender .
  ```
  
## 2. Запуск

Для запуска приложению нужна конфигурация в файле `CPMSender/appsettings.json` и две переменные окружения (POSTTRADE_DB и APIKEY).
APIKEY отвечает за доступ к апи индексов, а POSTTRADE_DB к доступу в базу данных сделок.

```bash
docker run --env APIKEY=<API_KEY> --env POSTTRADE_DB=<POSTTRADE_DB> --name cpmsender cpmsender 
```