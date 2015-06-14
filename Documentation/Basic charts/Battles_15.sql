SELECT  A.BattleYear, SUM(A.Infantry) as 'Infantry', SUM(A.Cavalry) as 'Cavalry', SUM(A.Artillery) as 'Artillery', SUM(A.Navy) as 'Navy'
FROM (SELECT YEAR(BA.StartDate) / 20 * 20 - 1000 AS BattleYear,BE.ConflictSide, SUM(BE.InfantryStrength) as Infantry, SUM(BE.CavalryStrength) as Cavalry, SUM(BE.ArtilleryStrength) as Artillery, SUM(Be.NavyStrength) as Navy
	FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BE
	WHERE BA.ID = BE.BattleID AND BA.StartDate IS NOT NULL
	GROUP BY BA.Name, BE.ConflictSide,YEAR(BA.StartDate) / 20 * 20 - 1000)
	AS A
GROUP BY BattleYear
ORDER BY BattleYear
