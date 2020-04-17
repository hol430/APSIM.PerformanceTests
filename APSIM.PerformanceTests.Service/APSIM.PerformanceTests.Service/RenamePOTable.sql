UPDATE PredictedObservedDetails
	SET TableName = @NewTableName
	WHERE ID IN (
	SELECT pod.ID 
		FROM ApsimFiles a 
		INNER JOIN PredictedObservedDetails pod ON a.ID = pod.ApsimFilesID
		WHERE pod.TableName = @OldTableName
		AND a.FileName = @FileName
);