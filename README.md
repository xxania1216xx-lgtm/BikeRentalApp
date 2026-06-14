# 🚲 BikeRentalApp - System Zarządzania Wypożyczalnią Rowerów

Nowoczesna aplikacja desktopowa typu **WPF (Windows Presentation Foundation)** stworzona w architekturze **MVVM**, służąca do kompleksowej obsługi wypożyczalni rowerów miejskich. Aplikacja charakteryzuje się nowoczesnym interfejsem w stylu **Windows 11** oraz zintegrowaną bazą danych **SQLite**.

## 🌟 Kluczowe Funkcje

### 👤 Panel Użytkownika
- **Rejestracja i Logowanie**: Prosty system autoryzacji.
- **Wypożyczanie**: Wybór stacji i konkretnego roweru z cennikiem dynamicznym.
- **Licznik Czasu**: Dynamiczny licznik (co do sekundy) pokazujący czas trwania aktywnego wypożyczenia.
- **Elastyczny Zwrot**: Możliwość oddania roweru na dowolną aktywną stację.
- **Zarządzanie Saldem**: Szybkie doładowanie konta (PLN).
- **Historia**: Przejrzysta lista wszystkich wypożyczeń z informacją o trasie (`Stacja A ➔ Stacja B`) i kosztach.
- **Zasada 24h**: Automatyczny zwrot roweru po 24h z naliczeniem kary (30 zł) w celu ochrony floty.

### 🛠️ Panel Administratora
- **Zarządzanie Stacjami**: Dodawanie, edycja nazw, włączanie/wyłączanie stacji oraz bezpieczne usuwanie (z automatycznym przenoszeniem rowerów).
- **Zarządzanie Flotą**: Dodawanie nowych rowerów, edycja modeli, zmiana stawek godzinowych oraz relokacja rowerów między stacjami.
- **Baza Użytkowników**: Pełna edycja danych, ręczna korekta salda, nadawanie uprawnień administratora oraz usuwanie kont.
- **Zakańczanie Wypożyczeń**: Automatyczne przerwanie aktywnego najmu przy usunięciu roweru z systemu.

---

## 🚀 Technologia
- **Język**: C#
- **Platforma**: .NET 10.0 Windows
- **Interfejs**: WPF (XAML) z customowymi stylami (Windows 11 Design)
- **Architektura**: Model-View-ViewModel (MVVM)
- **Baza danych**: Entity Framework Core + SQLite
- **Publikacja**: Skonfigurowane pod **Single File EXE** (wszystko w jednym pliku)

---

## 📦 Instalacja i Uruchomienie

Aplikacja jest gotowa do użycia natychmiast po pobraniu:

1. Przejdź do zakładki **Releases** w tym repozytorium.
2. Pobierz najnowszą wersję pliku `BikeRentalApp.exe`.
3. Uruchom pobrany plik. Baza danych zostanie utworzona automatycznie przy pierwszym starcie.

*Wskazówka: Program jest opublikowany jako „Single File EXE”, co oznacza, że nie wymaga instalacji żadnych dodatkowych bibliotek DLL.*

---

## 📖 Instrukcja Obsługi

### Pierwsze Kroki
- **Domyślny Administrator**: 
  - Login: `admin`
  - Hasło: `admin`
- **Rejestracja**: Kliknij "Załóż konto" na ekranie logowania, aby utworzyć profil użytkownika.

### Wypożyczanie Roweru (Użytkownik)
1. Doładuj konto w zakładce **Konto** (wymagane środki na min. 1h jazdy).
2. W zakładce **Wypożycz/Zwróć** wybierz stację z poziomego menu.
3. Zaznacz rower z listy i kliknij **Wypożycz**.
4. W prawym panelu pojawi się licznik czasu.

### Zwrot Roweru (Użytkownik)
1. Wybierz stację docelową z listy rozwijanej w panelu **Twoje aktualne wypożyczenie**.
2. Kliknij **Zwróć rower**. System automatycznie pobierze opłatę z Twojego salda.

### Zarządzanie Systemem (Admin)
- Przełączaj się między zakładkami **Stacje**, **Rowery** i **Użytkownicy**.
- Zawsze najpierw wybierz element z listy po lewej, aby edytować jego dane w panelu po prawej.
- Pamiętaj, aby po edycji kliknąć przycisk **Zapisz** lub **Zaktualizuj**.

---

## 🎨 Design i UX
Aplikacja została zaprojektowana z myślą o czytelności:
- **Style Windows 11**: Zaokrąglone rogi, cienie (DropShadow), "pływające" panele.
- **ListBox-based Tables**: Brak surowych, brzydkich tabel. Wszystkie dane są w estetycznych wierszach.
- **PLN Format**: Pełna obsługa polskiej waluty.
- **Statusy**: Kolorowe etykiety (np. zielona dla aktywnych stacji, niebieska dla administratorów).

---
*BikeRentalApp - Twoja miejska wolność w jednym kliknięciu.*
