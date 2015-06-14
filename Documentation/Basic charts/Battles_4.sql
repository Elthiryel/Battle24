SELECT Country,COUNT(NAME) as 'Liczba bitew' from BATTLES where Country is not null group by Country order by COUNT(NAME) DESC
