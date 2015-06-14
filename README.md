Struktura repozytorium:
BattleCrawler - projekt C# zawierający crawler do pozyskiwania i przetwarzania bitew, wojen i pokojów z angielskiej wikipedii

Data - dane przetworzone manualnie używane podczas procesu przetwarzania danych przez Crawler

Database - backup najnowszej bazy danych i skrypt tworzący tą bazę

Documentation/Basic charts - zawiera dane dotyczącce podstawowych statystyk w następującym formacie: dla każdego wykresu, opisanego w dokumentacji numerem, zawiera:
*   plik .sql zawierający query
*   plik .csv zawierający surowe dane z query
*   plik .xls zawierający już przetworzone dane do wykresu wraz z wykresem
*   plik .png zawierający wykres
*   plik .png zawierający tabelkę z danymi (opcjonalnie)

Dodatkowo plik tables.xls zawiera wszystkie tabelki

Documentation/Network analysis - zawiera dane dotyczące analiz sieci w następującym formacie: dla każdej sieci, opisanej w dokumentacji numerem, zawiera:
*   plik .csv zawierający surowe dane z query
*   plik .gephi zawierający projekt programu Gephi dla danej sieci
*   plik .xls zawierający czołówkę pagerank'u wraz z wykresem
*   plik .png zawierający wykres pagerank'u
*   plik .png zawierający wizualizację sieci

Documentation/Documentation.pdf - zawiera całą dokumentację projektu

	
