using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

public class BaseDataAccess
{
    protected string ConnectionString { get; set; }

    public BaseDataAccess()
    {
    }
    public BaseDataAccess(IConfiguration configuration)
    {
        this.ConnectionString = configuration["ConnectionStrings:ProdConnection"];
    }

    public BaseDataAccess(string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    private NpgsqlConnection GetConnection()
    {
        NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString);
        if (connection.State != ConnectionState.Open)
            connection.Open();
        return connection;
    }

    protected DbCommand GetCommand(DbConnection connection, string commandText, CommandType commandType)
    {
        var command = new NpgsqlCommand(commandText, connection as NpgsqlConnection);
        command.CommandType = commandType;
        return command;
    }

    protected NpgsqlParameter GetParameter(string parameter, object value)
    {
        var parameterObject = new NpgsqlParameter(parameter, value != null ? value : DBNull.Value);
        parameterObject.Direction = ParameterDirection.Input;
        return parameterObject;
    }

    protected NpgsqlParameter GetParameterOut(string parameter, SqlDbType type, object value = null, ParameterDirection parameterDirection = ParameterDirection.InputOutput)
    {
        var parameterObject = new NpgsqlParameter(parameter, type); ;

        if (type == SqlDbType.NVarChar || type == SqlDbType.VarChar || type == SqlDbType.NText || type == SqlDbType.Text)
        {
            parameterObject.Size = -1;
        }

        parameterObject.Direction = parameterDirection;

        if (value != null)
        {
            parameterObject.Value = value;
        }
        else
        {
            parameterObject.Value = DBNull.Value;
        }

        return parameterObject;
    }

    protected int ExecuteNonQuery(string procedureName, List<DbParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
    {
        int returnValue = -1;

        try
        {
            using (NpgsqlConnection connection = this.GetConnection())
            {
                DbCommand cmd = this.GetCommand(connection, procedureName, commandType);

                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                returnValue = cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            //LogException("Failed to ExecuteNonQuery for " + procedureName, ex, parameters);
            throw;
        }

        return returnValue;
    }

    protected object ExecuteScalar(string procedureName, List<NpgsqlParameter> parameters)
    {
        object returnValue = null;

        try
        {
            using (DbConnection connection = this.GetConnection())
            {
                DbCommand cmd = this.GetCommand(connection, procedureName, CommandType.StoredProcedure);

                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                returnValue = cmd.ExecuteScalar();
            }
        }
        catch (Exception ex)
        {
            //LogException("Failed to ExecuteScalar for " + procedureName, ex, parameters);
            throw;
        }

        return returnValue;
    }

    protected DbDataReader GetDataReader(string procedureName, List<DbParameter> parameters, CommandType commandType = CommandType.StoredProcedure)
    {
        DbDataReader ds;

        try
        {
            DbConnection connection = this.GetConnection();
            {
                DbCommand cmd = this.GetCommand(connection, procedureName, commandType);
                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                ds = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }
        catch (Exception ex)
        {
            //LogException("Failed to GetDataReader for " + procedureName, ex, parameters);
            throw;
        }

        return ds;
    }
}