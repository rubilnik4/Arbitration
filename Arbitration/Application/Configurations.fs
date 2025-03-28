module Arbitration.Application.Configurations

type ProjectConfig = {
    SpreadThreshold: decimal
    MaxHistorySize: int
}

type PostgresConfig = {
    ConnectionString: string
}

type Config = {
    Project: ProjectConfig
    Postgres: PostgresConfig    
}