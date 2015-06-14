SELECT Result, COUNT(*) AS ResultCount FROM
(
	SELECT LeftResult AS Result FROM
		(
		SELECT C.Name, C.AllStrength AS LeftStrength, D.AllStrength AS RightStrength, C.Result AS LeftResult, D.Result AS RightResult FROM
		(
			SELECT Name, ConflictSide, Result, SUM(AllStrength) AS AllStrength FROM
			(
				SELECT BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result
				FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
				WHERE BA.ID = BB.BattleID AND ConflictSide = 0
				GROUP BY BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result
			) AS A
			GROUP BY Name, ConflictSide, Result
		) AS C,
		(
			SELECT Name, ConflictSide, Result, SUM(AllStrength) AS AllStrength FROM
			(
				SELECT BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result
				FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
				WHERE BA.ID = BB.BattleID AND ConflictSide = 1
				GROUP BY BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result
			) AS B
			GROUP BY Name, ConflictSide, Result
		) AS D
		WHERE C.Name = D.Name
	) AS E
	WHERE LeftStrength > RightStrength
	UNION ALL
	SELECT RightResult AS Result FROM
		(
		SELECT C.Name, C.AllStrength AS LeftStrength, D.AllStrength AS RightStrength, C.Result AS LeftResult, D.Result AS RightResult FROM
		(
			SELECT Name, ConflictSide, Result, SUM(AllStrength) AS AllStrength FROM
			(
				SELECT BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result
				FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
				WHERE BA.ID = BB.BattleID AND ConflictSide = 0
				GROUP BY BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result
			) AS A
			GROUP BY Name, ConflictSide, Result
		) AS C,
		(
			SELECT Name, ConflictSide, Result, SUM(AllStrength) AS AllStrength FROM
			(
				SELECT BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result
				FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
				WHERE BA.ID = BB.BattleID AND ConflictSide = 1
				GROUP BY BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result
			) AS B
			GROUP BY Name, ConflictSide, Result
		) AS D
		WHERE C.Name = D.Name
	) AS E
	WHERE LeftStrength < RightStrength
) AS F
WHERE Result IS NOT NULL
GROUP BY Result
