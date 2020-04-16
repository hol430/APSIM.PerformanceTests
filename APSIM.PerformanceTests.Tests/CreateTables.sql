CREATE TABLE AcceptStatsLogs (
  ID INTEGER   PRIMARY KEY   NOT NULL,
  PullRequestId INTEGER   NOT NULL,
  SubmitPerson TEXT   NOT NULL,
  SubmitDate DATETIME   NOT NULL,
  LogPerson TEXT   NOT NULL,
  LogReason TEXT   NOT NULL,
  LogStatus BOOLEAN   NOT NULL,
  LogAcceptDate DATETIME,
  StatsPullRequestId INTEGER,
  FileCount INTEGER
);
CREATE TABLE ApsimFiles (
  ID INTEGER   PRIMARY KEY   NOT NULL,
  PullRequestId INTEGER   NOT NULL,
  FileName TEXT   NOT NULL,
  FullFileName TEXT   NOT NULL,
  RunDate TEXT   NOT NULL,
  StatsAccepted BOOLEAN,
  IsMerged BOOLEAN,
  SubmitDetails TEXT,
  AcceptedPullRequestId INTEGER,
  AcceptedRunDate TEXT
);
CREATE TABLE PredictedObservedDetails (
  ID INTEGER   PRIMARY KEY   NOT NULL,
  ApsimFilesID INTEGER   NOT NULL,
  TableName TEXT   NOT NULL,
  PredictedTableName TEXT   NOT NULL,
  ObservedTableName TEXT   NOT NULL,
  FieldNameUsedForMatch TEXT,
  FieldName2UsedForMatch TEXT,
  FieldName3UsedForMatch TEXT,
  PassedTests REAL,
  HasTests INTEGER,
  AcceptedPredictedObservedDetailsID INTEGER
);
CREATE TABLE PredictedObservedTests (
  ID INTEGER   PRIMARY KEY   NOT NULL,
  PredictedObservedDetailsID INTEGER   NOT NULL,
  Variable TEXT   NOT NULL,
  Test TEXT,
  Accepted REAL,
  Current REAL,
  Difference REAL,
  PassedTest BOOLEAN,
  AcceptedPredictedObservedTestsID INTEGER,
  IsImprovement BOOLEAN,
  SortOrder INTEGER,
  DifferencePercent REAL
);
CREATE TABLE PredictedObservedValues (
  ID INTEGER   PRIMARY KEY   NOT NULL,
  PredictedObservedDetailsID INTEGER   NOT NULL,
  SimulationsID INTEGER   NOT NULL,
  MatchName TEXT   NOT NULL,
  MatchValue REAL   NOT NULL,
  MatchName2 TEXT,
  MatchValue2 REAL,
  MatchName3 TEXT,
  MatchValue3 REAL,
  ValueName TEXT   NOT NULL,
  PredictedValue REAL,
  ObservedValue REAL
);
CREATE TABLE Simulations (
  ID INTEGER   PRIMARY KEY   NOT NULL,
  ApsimFilesID INTEGER   NOT NULL,
  Name TEXT   NOT NULL,
  OriginalSimulationID INTEGER   NOT NULL
);
