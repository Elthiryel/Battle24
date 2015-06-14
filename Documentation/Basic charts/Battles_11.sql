SELECT Leaders.Name, COUNT(Leaders.Name) as 'Liczba bitew' from LEADERS JOIN BATTLES_LEADERS on Leaders.ID = BATTLES_LEADERS.LeaderID
GROUP by Leaders.Name order by COUNT(Leaders.Name) DESC
