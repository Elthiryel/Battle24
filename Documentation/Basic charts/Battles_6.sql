SELECT Wars.Name, Count(WARS.Name) as 'Liczba bitew' from BATTLES join WARS on BATTLES.WarID = WARS.ID group by WARS.Name order by Count(WARS.Name) DESC
