using System;

using System.Collections.Generic;

using System.Linq;

using FMS.Infrastructure.Data.Implementation.IDataAccess.Repositories.Contabilidad;

using FMS.Application.DTO.Contabilidad.AsientoContable;

using FMS.Application.DTO.Shared;

 

using FMS.Shared.Enum;

using System.Data;

using System.Data.SqlClient;

using Oracle.ManagedDataAccess.Client;

using System.Text;

using FMS.Application.DTO.Contabilidad.CierreIdi;

using FMS.Application.DTO.Auditoria.AuditoriaFMS;

using FMS.Shared.Helper;

using FMS.Shared.Constants;

using FMS.Domain.Implementation.Contabilidad.Aggregates.CierreIdiAgg.ViewModel;

//using FMS.Infrastructure.Data.Implementation.IDataAccess.Repositories.Contabilidad;

using FMS.Infrastructure.Data.ImplementationDataAccess.Repositories.Contabilidad;

using FMS.Domain.Implementation.Contabilidad;

using System.Configuration;

namespace FMS.Infrastructure.Data.ImplementationDataAccess.Repositories.Contabilidad

{

    public class CierreIdiDataRepository : ICierreIdiDataRepository

    {

        #region Members

 

        IDapperDataAccess _dapperDataAccess;

        IEnvioComisionesExactusDataRepository iEnvioComisionesExactusDataRepository;

 

        #endregion

 

        #region Constructor

 

        public CierreIdiDataRepository(IDapperDataAccess dapperDataAccess)

        {

            if (dapperDataAccess == (IDapperDataAccess)null)

                throw new ArgumentNullException("dapperDataAccess");

 

            _dapperDataAccess = dapperDataAccess;

        }

 

        #endregion

        public void GuardarConexionExactus(Guid keyExactus, string conexionEncriptada, int idFechaActual)

        {

            List<SqlParameter> listaParametros = new List<SqlParameter>();

            listaParametros.Add(new SqlParameter { ParameterName = "@pKeyExactus", Value = keyExactus, SqlDbType = SqlDbType.UniqueIdentifier });

            listaParametros.Add(new SqlParameter { ParameterName = "@pConexionEncriptada", Value = conexionEncriptada, SqlDbType = SqlDbType.VarChar });

            listaParametros.Add(new SqlParameter { ParameterName = "@pIdFechaActual", Value = idFechaActual, SqlDbType = SqlDbType.Int });

            _dapperDataAccess.ExecuteSqlCommand("exec FMS.USP_GuardarConexionExactus @pKeyExactus,@pConexionEncriptada,@pIdFechaActual", listaParametros.ToArray());

        }

        public string ObtenerPrimerIdi()

        {

            List<SqlParameter> listaParametros = new List<SqlParameter>();

            SqlParameter IdSecuencialFecha = new SqlParameter { ParameterName = "@IdSecuencialFecha", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };

            listaParametros.Add(IdSecuencialFecha);

            _dapperDataAccess.SetCommandTimeout(0);

            _dapperDataAccess.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "EXEC FMS.ObtenerPrimerIdi @IdSecuencialFecha out", listaParametros.ToArray());

            return Helper.ConvertIdFechaToFechaString(Convert.ToInt32(IdSecuencialFecha.Value));

        }

 

        public UsuarioDatadDTO GetDatosUsuarios(UsuariosFilterDTO filter)

        {

            List<SqlParameter> listaParametros = new List<SqlParameter>();

            UsuarioDatadDTO list = _dapperDataAccess.SqlQuery<UsuarioDatadDTO>("SELECT U.CodigoEmpleado,UPPER(U.UsuarioNT) Usuario,ISNULL(RTRIM(UPPER(U.APELLIDOS) + ' ' + UPPER(U.NOMBRES)),SI.NOMBREEMPLEADO) NombreUsuario,ISNULL(SI.CENTROCOSTO,U.CENTRO_COSTO) CentroCosto FROM [dbo].[Usuario] U LEFT JOIN dbo.SEGURIDAD_INTRANET SI ON UPPER(SI.IDUSUARIO)=UPPER(U.UsuarioNT) WHERE UPPER(U.UsuarioNT) = UPPER('" + filter.Usuario + "')", listaParametros.ToArray()).FirstOrDefault();

 

            return list;

        }

 

 

        public UsuariosRolDTO GetDatosUsuariosRol(UsuariosFilterDTO filter)

        {

            string codigoAplicacion = ConfigurationManager.AppSettings["ServicioCodigoAplicacion"];

            List<SqlParameter> listaParametros = new List<SqlParameter>();

            UsuariosRolDTO UsuarioRolLista = new UsuariosRolDTO();

            var list = _dapperDataAccess.SqlQuery<UsuariosRolDTO>("SELECT Descripcion RolCodigo FROM dbo.Perfil_X_Usuario pu  inner join dbo.Perfil p ON p.PerfilID=pu.PerfilID inner join dbo.Usuario u ON u.UsuarioID=pu.UsuarioID INNER JOIN (SELECT AP.PerfilID FROM dbo.Aplicacion A INNER JOIN dbo.Aplicacion_X_Perfil  AP ON A.AplicacionID=AP.AplicacionID WHERE A.AplicacionID=" + codigoAplicacion + ") APL ON APL.PerfilID=p.PerfilID WHERE p.Habilitado = 1 AND u.UsuarioNT = '" + filter.Usuario + "' and pu.Estado = 'A'", listaParametros.ToArray());

            string nuevo = "";

 

            //string cadena = string.Join(",", list);

 

            try

            {

                foreach (var item in list)

                {

                    //UsuarioRolLista.RolCodigo = item.RolCodigo + ",";

                    nuevo += item.RolCodigo + ", ";

                }

                nuevo = nuevo.ToString().TrimEnd();

                nuevo = nuevo.Substring(0, nuevo.Length - 1);

                UsuarioRolLista.RolCodigo = nuevo;

            }

            catch (Exception)

            {

                UsuarioRolLista.RolCodigo = "";

            }

 

 

 

            return UsuarioRolLista;

        }

 

        public void ConfirmarValorCuotaConexionDirecta(int IdIdi, string Usuario, string CadenaConexion)

        {

            var SqlConnection = default(SqlConnection);

            var SqlTransaction = default(SqlTransaction);

            var OracleConnection = default(OracleConnection);

            var OracleTransaction = default(OracleTransaction);

 

            var DatosEnvioCuotaSysde = default(DatosEnvioCuotaSysde);

 

            try

            {

                SqlConnection = new SqlConnection(_dapperDataAccess.GetConectionString());

                SqlConnection.Open();

                SqlTransaction = SqlConnection.BeginTransaction();

 

                using (SqlCommand cmd = new SqlCommand("FMS.EnvioValorCuotaFMS", SqlConnection, SqlTransaction))

               {

                    cmd.CommandTimeout = 0;

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdIdi", IdIdi).Direction = ParameterDirection.Input;

                    cmd.Parameters.AddWithValue("@Usuario", Usuario).Direction = ParameterDirection.Input;

 

                    var parameterCodigoEnvio = cmd.Parameters.AddWithValue("@CodigoEnvio", "").Direction = ParameterDirection.Output;

 

                    using (var reader = cmd.ExecuteReader())

                    {

                        if (reader.Read())

                        {

                            DatosEnvioCuotaSysde = new DatosEnvioCuotaSysde()

                            {

                                CodigoContableFondo = Convert.ToInt32(reader["CodigoContableFondo"]),

                                CodigoEnvio = reader["CodigoEnvio"].ToString(),

                                CodigoFondo = reader["CodigoFondo"].ToString(),

                                CodigoGeneral = reader["CodigoGeneral"].ToString(),

                                IdFecha = Convert.ToInt32(reader["IdFecha"]),

                                ValorCuota = Convert.ToDecimal(reader["ValorCuota"]),

                            };

 

                            if (reader.NextResult())

                            {

                                while (reader.Read())

                                {

                                    DatosEnvioCuotaSysde.Rentabilidad.Add(new RentabilidadEnvioCuotaSysde

                                    {

                                        CodigoFondo = reader["CodigoFondo"].ToString(),

                                        CodigoRubro = reader["CodigoRubro"].ToString(),

                                        IdFecha = Convert.ToInt32(reader["IdSecuencialFecha"]),

                                        IdFechaActual = Convert.ToInt32(reader["IdFechaActual"]),

                                        IdHoraActual = Convert.ToInt32(reader["IdHoraActual"]),

                                        LoginActualizacion = reader["LoginActualizacion"].ToString(),

                                        UsuarioActualizacion = reader["Usuario"].ToString(),

                                        Rentabilidad = Convert.ToDecimal(reader["Rentabilidad"]),

                                        BasicPoint = Convert.ToDecimal(reader["BasicPoint"])

                                    });

                                }

                            }

 

                            if (reader.NextResult())

                            {

                                while (reader.Read())

                                {

                                    //try

                                    //{

 

                                        DatosEnvioCuotaSysde.Asientos.Add(new AsientoEnvioCuotaSysde

                                        {

                                            CodigoEmpresa = reader["CodigoEmpresa"].ToString(),

                                            CodigoGeneral = Convert.ToInt32(reader["CodigoGeneral"]),

                                            SecuencialDetalle = Convert.ToInt32(reader["SecuencialDetalle"]),

                                            SecuencialGrupo = Convert.ToInt32(reader["SecuencialGrupo"]),

                                            CuentaContable = reader["CuentaContable"].ToString(),

                                            TipoCambio = Convert.ToDecimal(reader["TipoCambio"]),

                                            TipoAsiento = reader["TipoAsiento"].ToString(),

                                            MontoMonedaOriginal = Convert.ToDecimal(reader["MontoMonedaOriginal"]),

                                            MontoMonedaSoles = Convert.ToDecimal(reader["MontoMonedaSoles"]),

                                            TotalCuotas = (reader["TotalCuotas"] == DBNull.Value) ? null : (Nullable<decimal>)Convert.ToDecimal(reader["TotalCuotas"]),

                                            GlosaCabecera = reader["GlosaCabecera"].ToString(),

                                            GlosaDetalle = reader["GlosaDetalle"].ToString(),

                                            Instrumento = reader["Instrumento"].ToString(),

                                            Referencia = reader["Referencia"].ToString(),

                                            Concepto = reader["Concepto"].ToString(),

                                            DetalleSBS = reader["DetalleSBS"].ToString(),

                                            IndicadorCaja = reader["IndicadorCaja"].ToString(),

                                            LoginActualizacion = reader["LoginActualizacion"].ToString(),

                                            CodigoEstado = reader["CodigoEstado"].ToString(),

                                            MotivoAnulacion = reader["MotivoAnulacion"].ToString(),

                                            TipoLinea = reader["TipoLinea"].ToString(),

                                            NumeroLinea = (reader["NumeroLinea"] == DBNull.Value) ? null : (Nullable<int>)Convert.ToInt32(reader["NumeroLinea"]),

                                            //NumeroAsiento = (reader["NumeroAsiento"] == DBNull.Value) ? null : (Nullable<int>)Convert.ToInt32(reader["NumeroAsiento"]),

                                            //psv:INC-20-0792 NumeroAsiento

                                            NumeroAsiento = (reader["NumeroAsiento"] == DBNull.Value) ? null : (Nullable<decimal>)Convert.ToDecimal(reader["NumeroAsiento"]),

                                            FechaHoraActualizacion = Convert.ToDateTime(reader["FechaHoraActualizacion"])

                                        });

 

                                    //}

                                    //catch (Exception ex)

                                    //{

                                    //    string msje = ex.Message;

                                    //    throw ex;

                                    //}

                                }

                            }

 

                            if (reader.NextResult())

                            {

                                while (reader.Read())

                                {

                                    DatosEnvioCuotaSysde.DiasValorCuota.Add(new DiasValorCuotaEnvioCuotaSysde

                                    {

                                        CodigoContableFondo = Convert.ToInt32(reader["CodigoContableFondo"]),

                                        CodigoEnvio = reader["CodigoEnvio"].ToString(),

                                        CodigoFondo = reader["CodigoFondo"].ToString(),

                                        CodigoGeneral = reader["CodigoGeneral"].ToString(),

                                        IdFecha = Convert.ToInt32(reader["IdFecha"]),

                                        ValorCuota = Convert.ToDecimal(reader["ValorCuota"]),

                                    });

                                }

                            }

                        }

                    }

                }

 

                OracleConnection = new OracleConnection(CadenaConexion);

                OracleConnection.Open();

                OracleTransaction = OracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);

 

                using (var OracleCommand = OracleConnection.CreateCommand())

                {

                    var Consulta = new StringBuilder();

                    Consulta.AppendLine("SELECT COD_EMPRESA,");

                    Consulta.AppendLine("       SEQ_GENERACION,");

                    Consulta.AppendLine("       COD_INVERSION,");

                    Consulta.AppendLine("       TO_CHAR(FEC_OPERACION,'YYYYMMDD') AS FEC_OPERACION,");

                    Consulta.AppendLine("       NOM_ARCHIVO,");

                    Consulta.AppendLine("       COD_ESTADO");

                    Consulta.AppendLine("FROM PP_CG_ENC_AS_MASIVO");

                    Consulta.AppendLine("WHERE  COD_INVERSION = '" + DatosEnvioCuotaSysde.CodigoContableFondo.ToString() + "' AND");

                    Consulta.AppendLine("       COD_ESTADO = 'TR' AND");

                    Consulta.AppendLine("       TO_CHAR(FEC_OPERACION,'YYYYMMDD') = '" + DatosEnvioCuotaSysde.IdFecha.ToString() + "'");

 

                    OracleCommand.CommandTimeout = 0;

                    OracleCommand.CommandType = CommandType.Text;

                    OracleCommand.CommandText = Consulta.ToString();

 

                    using (var reader = OracleCommand.ExecuteReader())

                    {

                        if (reader.Read())

                            throw new ApplicationException("Es necesario reversar la Transferencia de Asientos en el Sysde para reversar el Valor Cuota");

                    }

                }

 

                using (var OracleCommand = OracleConnection.CreateCommand())

                {

                    var Consulta = new StringBuilder();

                    Consulta.AppendLine("SELECT COD_EMPRESA,");

                    Consulta.AppendLine("       SEQ_GENERACION,");

                    Consulta.AppendLine("       COD_INVERSION,");

                    Consulta.AppendLine("       TO_CHAR(FEC_OPERACION,'YYYYMMDD') AS FEC_OPERACION,");

                    Consulta.AppendLine("       NOM_ARCHIVO,");

                    Consulta.AppendLine("       COD_ESTADO");

                    Consulta.AppendLine("FROM PP_CG_ENC_AS_MASIVO");

                    Consulta.AppendLine("WHERE  COD_INVERSION = '" + DatosEnvioCuotaSysde.CodigoContableFondo.ToString() + "' AND");

                    Consulta.AppendLine("       COD_ESTADO = 'PE' AND");

                    Consulta.AppendLine("       TO_CHAR(FEC_OPERACION,'YYYYMMDD') = '" + DatosEnvioCuotaSysde.IdFecha.ToString() + "'");

 

                    OracleCommand.CommandTimeout = 0;

                    OracleCommand.CommandType = CommandType.Text;

                    OracleCommand.CommandText = Consulta.ToString();

 

                    using (var reader = OracleCommand.ExecuteReader())

                    {

                        while (reader.Read())

                        {

                            using (var OracleCommandDelete = OracleConnection.CreateCommand())

                            {

                                var ConsultaDelete = new StringBuilder();

                                ConsultaDelete.AppendLine("DELETE FROM PP_CG_DET_AS_MASIVO ");

                                ConsultaDelete.AppendLine("WHERE COD_EMPRESA = '" + reader["COD_EMPRESA"].ToString().Trim() + "' AND");

                                ConsultaDelete.AppendLine("      SEQ_GENERACION = '" + reader["SEQ_GENERACION"].ToString().Trim() + "'");

 

                                OracleCommandDelete.CommandTimeout = 0;

                                OracleCommandDelete.CommandType = CommandType.Text;

                                OracleCommandDelete.CommandText = ConsultaDelete.ToString();

 

                                OracleCommandDelete.ExecuteNonQuery();

                            }

                            using (var OracleCommandDelete = OracleConnection.CreateCommand())

                            {

                                var ConsultaDelete = new StringBuilder();

                                ConsultaDelete.AppendLine("DELETE FROM PP_CG_ENC_AS_MASIVO ");

                                ConsultaDelete.AppendLine("WHERE COD_EMPRESA = '" + reader["COD_EMPRESA"].ToString().Trim() + "' AND");

                                ConsultaDelete.AppendLine("      SEQ_GENERACION = '" + reader["SEQ_GENERACION"].ToString().Trim() + "'");

 

                                OracleCommandDelete.CommandTimeout = 0;

                                OracleCommandDelete.CommandType = CommandType.Text;

                                OracleCommandDelete.CommandText = ConsultaDelete.ToString();

 

                                OracleCommandDelete.ExecuteNonQuery();

                            }

                        }

                    }

                }

 

                foreach (var item in DatosEnvioCuotaSysde.DiasValorCuota)

                {

                    using (var OracleCommand = OracleConnection.CreateCommand())

                    {

                        var Consulta = new StringBuilder();

                        Consulta.AppendLine("SELECT COD_EMPRESA,");

                        Consulta.AppendLine("       FEC_VALOR,");

                        Consulta.AppendLine("       COD_INVERSION");

                        Consulta.AppendLine("FROM PP_FO_VALORES");

                        Consulta.AppendLine("WHERE  COD_INVERSION = '" + item.CodigoFondo + "' AND");

                        Consulta.AppendLine("       TO_CHAR(FEC_VALOR,'YYYYMMDD') = '" + item.IdFecha.ToString() + "'");

 

                        OracleCommand.CommandTimeout = 0;

                        OracleCommand.CommandType = CommandType.Text;

                        OracleCommand.CommandText = Consulta.ToString();

 

                        using (var reader = OracleCommand.ExecuteReader())

                        {

                            if (!reader.Read())

                            {

                                using (var OracleCommandInsert = OracleConnection.CreateCommand())

                                {

                                    var ConsultaInsert = new StringBuilder();

                                    ConsultaInsert.AppendLine("INSERT INTO PP_FO_VALORES(FEC_VALOR,");

                                    ConsultaInsert.AppendLine("                          COD_INVERSION,");

                                    ConsultaInsert.AppendLine("                          COD_EMPRESA,");

                                    ConsultaInsert.AppendLine("                          VALOR,");

                                    ConsultaInsert.AppendLine("                          VALOR_CUOTA_BRUTO, ");

                                    ConsultaInsert.AppendLine("                          VALOR_NETO,");

                                    ConsultaInsert.AppendLine("                          VALOR_COMISION,");

                                    ConsultaInsert.AppendLine("                          NUM_CUOTAS,");

                                    ConsultaInsert.AppendLine("                          VALOR_NETO_BRUTO,");

                                    ConsultaInsert.AppendLine("                          VALOR_COMISION_REAL,");

                                    ConsultaInsert.AppendLine("                          VALOR_RENTABILIDAD,");

                                    ConsultaInsert.AppendLine("                          NUM_ASIENTO_COMISION,");

                                    ConsultaInsert.AppendLine("                          NUM_ASIENTO_RENTAB,");

                                    ConsultaInsert.AppendLine("                          INCLUIDO_POR ,");

                                    ConsultaInsert.AppendLine("                          FEC_INCLUSION,");

                                    ConsultaInsert.AppendLine("                          MODIFICADO_POR, ");

                                    ConsultaInsert.AppendLine("                          FEC_MODIFICACION,");

                                    ConsultaInsert.AppendLine("                          VALOR_FONDO,");

                                    ConsultaInsert.AppendLine("                          VALOR_CUOTA_BRUTO_HIS,");

                                    ConsultaInsert.AppendLine("                          IND_AUTORIZADO) ");

 

                                    ConsultaInsert.AppendLine("SELECT   TO_DATE('" + item.IdFecha.ToString() + "','YYYYMMDD'),");

                                    ConsultaInsert.AppendLine("         '" + item.CodigoFondo + "',");

                                    ConsultaInsert.AppendLine("         2,");

                                    ConsultaInsert.AppendLine("         " + item.ValorCuota.ToString() + ",");

                                    ConsultaInsert.AppendLine("         " + item.ValorCuota.ToString() + ",");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         '" + Usuario + "',");

                                    ConsultaInsert.AppendLine("         SYSDATE,");

                                    ConsultaInsert.AppendLine("         '" + Usuario + "',");

                                    ConsultaInsert.AppendLine("         SYSDATE,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         0,");

                                    ConsultaInsert.AppendLine("         'S'");

                                    ConsultaInsert.AppendLine("FROM DUAL");

 

 

                                    OracleCommandInsert.CommandTimeout = 0;

                                    OracleCommandInsert.CommandType = CommandType.Text;

                                    OracleCommandInsert.CommandText = ConsultaInsert.ToString();

 

                                    OracleCommandInsert.ExecuteNonQuery();

                                }

                            }

                            else

                            {

                                using (var OracleCommandUpdate = OracleConnection.CreateCommand())

                                {

                                    var ConsultaUpdate = new StringBuilder();

                                    ConsultaUpdate.AppendLine("UPDATE PP_FO_VALORES ");

                                    ConsultaUpdate.AppendLine("     SET COD_EMPRESA = '2',");

                                    ConsultaUpdate.AppendLine("         VALOR = " + item.ValorCuota.ToString() + ",");

                                    ConsultaUpdate.AppendLine("         VALOR_CUOTA_BRUTO = " + item.ValorCuota.ToString() + ",");

                                    ConsultaUpdate.AppendLine("         MODIFICADO_POR = '" + Usuario + "',");

                                    ConsultaUpdate.AppendLine("         FEC_MODIFICACION = SYSDATE,");

                                    ConsultaUpdate.AppendLine("         IND_AUTORIZADO = 'S' ");

                                    ConsultaUpdate.AppendLine("WHERE  COD_INVERSION = '" + item.CodigoFondo + "' AND");

                                    ConsultaUpdate.AppendLine("       TO_CHAR(FEC_VALOR,'YYYYMMDD') = '" + item.IdFecha.ToString() + "'");

 

                                    OracleCommandUpdate.CommandTimeout = 0;

                                    OracleCommandUpdate.CommandType = CommandType.Text;

                                    OracleCommandUpdate.CommandText = ConsultaUpdate.ToString();

 

                                    OracleCommandUpdate.ExecuteNonQuery();

                                }

                            }

                        }

                    }

                }

 

                using (var OracleCommandDelete = OracleConnection.CreateCommand())

                {

                    var ConsultaDelete = new StringBuilder();

                    ConsultaDelete.AppendLine("DELETE FROM CLIFAC ");

                    ConsultaDelete.AppendLine("WHERE FAC_TIP_FDO = '" + DatosEnvioCuotaSysde.CodigoFondo + "' AND");

                    ConsultaDelete.AppendLine("      FAC_FEC_REN = '" + DatosEnvioCuotaSysde.IdFecha + "'");

 

                    OracleCommandDelete.CommandTimeout = 0;

                    OracleCommandDelete.CommandType = CommandType.Text;

                    OracleCommandDelete.CommandText = ConsultaDelete.ToString();

 

                    OracleCommandDelete.ExecuteNonQuery();

                }

 

                using (var OracleCommandInsert = OracleConnection.CreateCommand())

                {

                    var ConsultaInsert = new StringBuilder();

                    ConsultaInsert.AppendLine("INSERT INTO CLIFAC(  FAC_TIP_FDO,");

                    ConsultaInsert.AppendLine("                     FAC_FEC_REN,");

                    ConsultaInsert.AppendLine("                     FAC_RUB_VSF,");

                    ConsultaInsert.AppendLine("                     FAC_TOT_GRL,");

                    ConsultaInsert.AppendLine("                     FAC_BAS_POI_GRL,");

                    ConsultaInsert.AppendLine("                     FAC_FEC_ING,");

                    ConsultaInsert.AppendLine("                     FAC_COD_USU,");

                    ConsultaInsert.AppendLine("                     FAC_NOM_PRG,");

                    ConsultaInsert.AppendLine("                     FAC_FEC_ULT_MAN,");

                    ConsultaInsert.AppendLine("                     FAC_HOR_ULT_MAN,");

                    ConsultaInsert.AppendLine("                     FAC_HOR_ING )");

                    ConsultaInsert.AppendLine("VALUES (:FAC_TIP_FDO,");

                    ConsultaInsert.AppendLine("        :FAC_FEC_REN,");

                    ConsultaInsert.AppendLine("        :FAC_RUB_VSF,");

                    ConsultaInsert.AppendLine("        :FAC_TOT_GRL,");

                    ConsultaInsert.AppendLine("        :FAC_BAS_POI_GRL,");

                    ConsultaInsert.AppendLine("        :FAC_FEC_ING,");

                    ConsultaInsert.AppendLine("        :FAC_COD_USU,");

                    ConsultaInsert.AppendLine("        :FAC_NOM_PRG,");

                    ConsultaInsert.AppendLine("        :FAC_FEC_ULT_MAN,");

                    ConsultaInsert.AppendLine("        :FAC_HOR_ULT_MAN,");

                    ConsultaInsert.AppendLine("        :FAC_HOR_ING )");

 

 

                    OracleCommandInsert.CommandTimeout = 0;

                    OracleCommandInsert.CommandType = CommandType.Text;

                    OracleCommandInsert.CommandText = ConsultaInsert.ToString();

                    OracleCommandInsert.BindByName = true;

 

                    OracleCommandInsert.ArrayBindCount = DatosEnvioCuotaSysde.Rentabilidad.Count;

 

                    OracleCommandInsert.Parameters.Add(":FAC_TIP_FDO", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.CodigoFondo).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_FEC_REN", OracleDbType.Int32, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.IdFecha).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_RUB_VSF", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.CodigoRubro).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_TOT_GRL", OracleDbType.Decimal, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.Rentabilidad).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_BAS_POI_GRL", OracleDbType.Decimal, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.BasicPoint).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_FEC_ING", OracleDbType.Int32, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.IdFechaActual).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_COD_USU", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.LoginActualizacion).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_NOM_PRG", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.UsuarioActualizacion).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_FEC_ULT_MAN", OracleDbType.Int32, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.IdFechaActual).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_HOR_ULT_MAN", OracleDbType.Int32, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.IdHoraActual).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FAC_HOR_ING", OracleDbType.Int32, DatosEnvioCuotaSysde.Rentabilidad.Select(c => c.IdHoraActual).ToArray(), ParameterDirection.Input);

 

                    OracleCommandInsert.ExecuteNonQuery();

                }

 

                using (var OracleCommandInsert = OracleConnection.CreateCommand())

                {

                    var ConsultaInsert = new StringBuilder();

                    ConsultaInsert.AppendLine("INSERT INTO PP_CG_ENC_AS_MASIVO( COD_EMPRESA,");

                    ConsultaInsert.AppendLine("                                 SEQ_GENERACION,");

                    ConsultaInsert.AppendLine("                                 COD_INVERSION,");

                    ConsultaInsert.AppendLine("                                 FEC_OPERACION,");

                    ConsultaInsert.AppendLine("                                 NOM_ARCHIVO,");

                    ConsultaInsert.AppendLine("                                 INCLUIDO_POR,");

                    ConsultaInsert.AppendLine("                                 FEC_INCLUSION,");

                    ConsultaInsert.AppendLine("                                 MODIFICADO_POR, ");

                    ConsultaInsert.AppendLine("                                 FEC_MODIFICACION,");

                    ConsultaInsert.AppendLine("                                 COD_ESTADO )");

 

                    ConsultaInsert.AppendLine("VALUES(  2,");

                    ConsultaInsert.AppendLine("         " + DatosEnvioCuotaSysde.CodigoGeneral + ",");

                    ConsultaInsert.AppendLine("         " + DatosEnvioCuotaSysde.CodigoContableFondo.ToString() + ",");

                    ConsultaInsert.AppendLine("         TO_DATE('" + DatosEnvioCuotaSysde.IdFecha.ToString() + "','YYYYMMDD'),");

                    ConsultaInsert.AppendLine("         '" + DatosEnvioCuotaSysde.CodigoEnvio + "',");

                    ConsultaInsert.AppendLine("         '" + Usuario + "',");

                    ConsultaInsert.AppendLine("         SYSDATE,");

                    ConsultaInsert.AppendLine("         '',");

                    ConsultaInsert.AppendLine("         '',");

                    ConsultaInsert.AppendLine("         'PE')");

 

                    OracleCommandInsert.CommandTimeout = 0;

                    OracleCommandInsert.CommandType = CommandType.Text;

                    OracleCommandInsert.CommandText = ConsultaInsert.ToString();

 

                    OracleCommandInsert.ExecuteNonQuery();

                }

 

                using (var OracleCommandInsert = OracleConnection.CreateCommand())

                {

                    var ConsultaInsert = new StringBuilder();

                    ConsultaInsert.AppendLine("INSERT INTO PP_CG_DET_AS_MASIVO(  COD_EMPRESA,");

                    ConsultaInsert.AppendLine("                                  SEQ_GENERACION,");

                    ConsultaInsert.AppendLine("                                  SEQ_DETALLE,");

                    ConsultaInsert.AppendLine("                                  GRUPO,");

                    ConsultaInsert.AppendLine("                                  CUENTA_CONTABLE,");

                    ConsultaInsert.AppendLine("                                  TIPO_MOV,");

                    ConsultaInsert.AppendLine("                                  TC_ORIGEN,");

                    ConsultaInsert.AppendLine("                                  MONTO_ORIGEN,");

                    ConsultaInsert.AppendLine("                                  MONTO_SOLES,");

                    ConsultaInsert.AppendLine("                                  TOT_CUOTA,");

                    ConsultaInsert.AppendLine("                                  GLOSA_CABECERA,");

                    ConsultaInsert.AppendLine("                                  GLOSA_DETALLE,");

                    ConsultaInsert.AppendLine("                                  INSTRUMENTO,");

                    ConsultaInsert.AppendLine("                                  REFERENCIA,");

                    ConsultaInsert.AppendLine("                                  CONCEPTO,");

                    ConsultaInsert.AppendLine("                                  DETALLE_SBS,");

                    ConsultaInsert.AppendLine("                                  IND_CAJA,");

                    ConsultaInsert.AppendLine("                                  INCLUIDO_POR,");

                    ConsultaInsert.AppendLine("                                  FEC_INCLUSION,");

                    ConsultaInsert.AppendLine("                                  MODIFICADO_POR,");

                    ConsultaInsert.AppendLine("                                  FEC_MODIFICACION,");

                    ConsultaInsert.AppendLine("                                  COD_ESTADO,");

                    ConsultaInsert.AppendLine("                                  MOT_ANULACION,");

                    ConsultaInsert.AppendLine("                                  NUM_ASIENTO,");

                    ConsultaInsert.AppendLine("                                  NUM_LINEA, ");

                    ConsultaInsert.AppendLine("                                  TIP_LINEA )");

                    ConsultaInsert.AppendLine("VALUES (:COD_EMPRESA,");

                    ConsultaInsert.AppendLine("        :SEQ_GENERACION,");

                    ConsultaInsert.AppendLine("        :SEQ_DETALLE,");

                    ConsultaInsert.AppendLine("        :GRUPO,");

                    ConsultaInsert.AppendLine("        :CUENTA_CONTABLE,");

                    ConsultaInsert.AppendLine("        :TIPO_MOV,");

                    ConsultaInsert.AppendLine("        :TC_ORIGEN,");

                    ConsultaInsert.AppendLine("        :MONTO_ORIGEN,");

                    ConsultaInsert.AppendLine("        :MONTO_SOLES,");

                    ConsultaInsert.AppendLine("        :TOT_CUOTA,");

                    ConsultaInsert.AppendLine("        :GLOSA_CABECERA,");

                    ConsultaInsert.AppendLine("        :GLOSA_DETALLE,");

                    ConsultaInsert.AppendLine("        :INSTRUMENTO,");

                    ConsultaInsert.AppendLine("        :REFERENCIA,");

                    ConsultaInsert.AppendLine("        :CONCEPTO,");

                    ConsultaInsert.AppendLine("        :DETALLE_SBS,");

                    ConsultaInsert.AppendLine("        :IND_CAJA,");

                    ConsultaInsert.AppendLine("        :INCLUIDO_POR,");

                    ConsultaInsert.AppendLine("        :FEC_INCLUSION,");

                    ConsultaInsert.AppendLine("        :MODIFICADO_POR,");

                    ConsultaInsert.AppendLine("        :FEC_MODIFICACION,");

                    ConsultaInsert.AppendLine("        :COD_ESTADO,");

                    ConsultaInsert.AppendLine("        :MOT_ANULACION,");

                    ConsultaInsert.AppendLine("        :NUM_ASIENTO,");

                    ConsultaInsert.AppendLine("        :NUM_LINEA, ");

                    ConsultaInsert.AppendLine("        :TIP_LINEA )");

 

                    OracleCommandInsert.CommandTimeout = 0;

                    OracleCommandInsert.CommandType = CommandType.Text;

                    OracleCommandInsert.CommandText = ConsultaInsert.ToString();

                    OracleCommandInsert.BindByName = true;

 

                    OracleCommandInsert.ArrayBindCount = DatosEnvioCuotaSysde.Asientos.Count;

 

                    OracleCommandInsert.Parameters.Add(":COD_EMPRESA", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.CodigoEmpresa).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":SEQ_GENERACION", OracleDbType.Int32, DatosEnvioCuotaSysde.Asientos.Select(c => c.CodigoGeneral).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":SEQ_DETALLE", OracleDbType.Int32, DatosEnvioCuotaSysde.Asientos.Select(c => c.SecuencialDetalle).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":GRUPO", OracleDbType.Int32, DatosEnvioCuotaSysde.Asientos.Select(c => c.SecuencialGrupo).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":CUENTA_CONTABLE", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.CuentaContable).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":TIPO_MOV", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.TipoAsiento).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":TC_ORIGEN", OracleDbType.Decimal, DatosEnvioCuotaSysde.Asientos.Select(c => c.TipoCambio).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":MONTO_ORIGEN", OracleDbType.Decimal, DatosEnvioCuotaSysde.Asientos.Select(c => c.MontoMonedaOriginal).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":MONTO_SOLES", OracleDbType.Decimal, DatosEnvioCuotaSysde.Asientos.Select(c => c.MontoMonedaSoles).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":TOT_CUOTA", OracleDbType.Decimal, DatosEnvioCuotaSysde.Asientos.Select(c => c.TotalCuotas).ToArray(), ParameterDirection.Input);

 

                    OracleCommandInsert.Parameters.Add(":GLOSA_CABECERA", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.GlosaCabecera).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":GLOSA_DETALLE", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.GlosaDetalle).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":INSTRUMENTO", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.Instrumento).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":REFERENCIA", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.Referencia).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":CONCEPTO", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.Concepto).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":DETALLE_SBS", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.DetalleSBS).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":IND_CAJA", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.IndicadorCaja).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":INCLUIDO_POR", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.LoginActualizacion).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FEC_INCLUSION", OracleDbType.Date, DatosEnvioCuotaSysde.Asientos.Select(c => c.FechaHoraActualizacion).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":MODIFICADO_POR", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.LoginActualizacion).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":FEC_MODIFICACION", OracleDbType.Date, DatosEnvioCuotaSysde.Asientos.Select(c => c.FechaHoraActualizacion).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":COD_ESTADO", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.CodigoEstado).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":MOT_ANULACION", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.MotivoAnulacion).ToArray(), ParameterDirection.Input);

                    //PSV:INC-20-0792 NumeroAsiento Revisar

                    OracleCommandInsert.Parameters.Add(":NUM_ASIENTO", OracleDbType.Decimal, DatosEnvioCuotaSysde.Asientos.Select(c => c.NumeroAsiento).ToArray(), ParameterDirection.Input);

                    //OracleCommandInsert.Parameters.Add(":NUM_ASIENTO", OracleDbType.Int32, DatosEnvioCuotaSysde.Asientos.Select(c => c.NumeroAsiento).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":NUM_LINEA", OracleDbType.Int32, DatosEnvioCuotaSysde.Asientos.Select(c => c.NumeroLinea).ToArray(), ParameterDirection.Input);

                    OracleCommandInsert.Parameters.Add(":TIP_LINEA", OracleDbType.Varchar2, DatosEnvioCuotaSysde.Asientos.Select(c => c.TipoLinea).ToArray(), ParameterDirection.Input);

 

                    OracleCommandInsert.ExecuteNonQuery();

                }

                SqlTransaction.Commit();

                OracleTransaction.Commit();

            }

            catch (Exception ex)

            {

                if (SqlTransaction != null)

                    SqlTransaction.Rollback();

 

                if (OracleTransaction != null)

                    OracleTransaction.Rollback();

 

                throw ex;

            }

            finally

            {

                if (SqlTransaction != null)

                    SqlTransaction.Dispose();

 

                if (OracleTransaction != null)

                    OracleTransaction.Dispose();

 

                if (OracleConnection != null)

                    if (OracleConnection.State == ConnectionState.Open)

                        OracleConnection.Close();

 

                if (SqlConnection != null)

                    if (SqlConnection.State == ConnectionState.Open)

                        SqlConnection.Close();

 

                if (SqlConnection != null)

                    SqlConnection.Dispose();

 

                if (OracleConnection != null)

                    OracleConnection.Dispose();

 

            }

        }

 

        public void ReversarValorCuotaConexionDirecta(int IdIdi, string Usuario, string CadenaConexion)

        {

            var SqlConnection = default(SqlConnection);

            var SqlTransaction = default(SqlTransaction);

            var OracleConnection = default(OracleConnection);

            var OracleTransaction = default(OracleTransaction);

            var DatosEnvioCuotaSysde = default(DatosEnvioCuotaSysde);

 

            try

            {

                SqlConnection = new SqlConnection(_dapperDataAccess.GetConectionString());

                SqlConnection.Open();

                SqlTransaction = SqlConnection.BeginTransaction();

 

                using (SqlCommand cmd = new SqlCommand("FMS.ReversarEnvioValorCuotaFMS", SqlConnection, SqlTransaction))

                {

                    cmd.CommandTimeout = 0;

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdIdi", IdIdi).Direction = ParameterDirection.Input;

                    cmd.Parameters.AddWithValue("@Usuario", Usuario).Direction = ParameterDirection.Input;

 

                    using (var reader = cmd.ExecuteReader())

                    {

                        if (reader.Read())

                        {

                            DatosEnvioCuotaSysde = new DatosEnvioCuotaSysde()

                            {

                                CodigoContableFondo = Convert.ToInt32(reader["CodigoContableFondo"]),

                                CodigoEnvio = reader["CodigoEnvio"].ToString(),

                                CodigoFondo = reader["CodigoFondo"].ToString(),

                                CodigoGeneral = reader["CodigoGeneral"].ToString(),

                                IdFecha = Convert.ToInt32(reader["IdFecha"]),

                                ValorCuota = Convert.ToDecimal(reader["ValorCuota"]),

                            };

 

                            if (reader.NextResult())

                            {

                                while (reader.Read())

                                {

                                    DatosEnvioCuotaSysde.DiasValorCuota.Add(new DiasValorCuotaEnvioCuotaSysde

                                    {

                                        CodigoContableFondo = Convert.ToInt32(reader["CodigoContableFondo"]),

                                        CodigoEnvio = reader["CodigoEnvio"].ToString(),

                                        CodigoFondo = reader["CodigoFondo"].ToString(),

                                        CodigoGeneral = reader["CodigoGeneral"].ToString(),

                                        IdFecha = Convert.ToInt32(reader["IdFecha"]),

                                        ValorCuota = Convert.ToDecimal(reader["ValorCuota"]),

                                    });

                                }

                            }

                        }

                    }

                }

 

                OracleConnection = new OracleConnection(CadenaConexion);

                OracleConnection.Open();

                OracleTransaction = OracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);

 

                using (var OracleCommand = OracleConnection.CreateCommand())

                {

                    var Consulta = new StringBuilder();

                    Consulta.AppendLine("SELECT COD_EMPRESA,");

                    Consulta.AppendLine("       SEQ_GENERACION,");

                    Consulta.AppendLine("       COD_INVERSION,");

                    Consulta.AppendLine("       TO_CHAR(FEC_OPERACION,'YYYYMMDD') AS FEC_OPERACION,");

                    Consulta.AppendLine("       NOM_ARCHIVO,");

                    Consulta.AppendLine("       COD_ESTADO");

                    Consulta.AppendLine("FROM PP_CG_ENC_AS_MASIVO");

                    Consulta.AppendLine("WHERE  COD_INVERSION = '" + DatosEnvioCuotaSysde.CodigoContableFondo.ToString() + "' AND");

                    Consulta.AppendLine("       COD_ESTADO = 'TR' AND");

                    Consulta.AppendLine("       TO_CHAR(FEC_OPERACION,'YYYYMMDD') = '" + DatosEnvioCuotaSysde.IdFecha.ToString() + "'");

 

                    OracleCommand.CommandTimeout = 0;

                    OracleCommand.CommandType = CommandType.Text;

                    OracleCommand.CommandText = Consulta.ToString();

 

                    using (var reader = OracleCommand.ExecuteReader())

                    {

                        if (reader.Read())

                            throw new ApplicationException("Es necesario reversar la Transferencia de Asientos en el Sysde para reversar el Valor Cuota");

                    }

                }

 

                using (var OracleCommand = OracleConnection.CreateCommand())

                {

                    var Consulta = new StringBuilder();

                    Consulta.AppendLine("SELECT COD_EMPRESA,");

                    Consulta.AppendLine("       SEQ_GENERACION,");

                    Consulta.AppendLine("       COD_INVERSION,");

                    Consulta.AppendLine("       TO_CHAR(FEC_OPERACION,'YYYYMMDD') AS FEC_OPERACION,");

                    Consulta.AppendLine("       NOM_ARCHIVO,");

                    Consulta.AppendLine("       COD_ESTADO");

                    Consulta.AppendLine("FROM PP_CG_ENC_AS_MASIVO");

                    Consulta.AppendLine("WHERE  COD_INVERSION = '" + DatosEnvioCuotaSysde.CodigoContableFondo.ToString() + "' AND");

                    Consulta.AppendLine("       COD_ESTADO = 'PE' AND");

                    Consulta.AppendLine("       TO_CHAR(FEC_OPERACION,'YYYYMMDD') = '" + DatosEnvioCuotaSysde.IdFecha.ToString() + "'");

 

                    OracleCommand.CommandTimeout = 0;

                    OracleCommand.CommandType = CommandType.Text;

                    OracleCommand.CommandText = Consulta.ToString();

 

                    using (var reader = OracleCommand.ExecuteReader())

                    {

                        while (reader.Read())

                        {

                            using (var OracleCommandDelete = OracleConnection.CreateCommand())

                            {

                                var ConsultaDelete = new StringBuilder();

                                ConsultaDelete.AppendLine("DELETE FROM PP_CG_DET_AS_MASIVO ");

                                ConsultaDelete.AppendLine("WHERE COD_EMPRESA = '" + reader["COD_EMPRESA"].ToString().Trim() + "' AND");

                                ConsultaDelete.AppendLine("      SEQ_GENERACION = '" + reader["SEQ_GENERACION"].ToString().Trim() + "'");

 

                                OracleCommandDelete.CommandTimeout = 0;

                                OracleCommandDelete.CommandType = CommandType.Text;

                                OracleCommandDelete.CommandText = ConsultaDelete.ToString();

 

                                OracleCommandDelete.ExecuteNonQuery();

                            }

 

                            using (var OracleCommandDelete = OracleConnection.CreateCommand())

                            {

                                var ConsultaDelete = new StringBuilder();

                                ConsultaDelete.AppendLine("DELETE FROM PP_CG_ENC_AS_MASIVO ");

                                ConsultaDelete.AppendLine("WHERE COD_EMPRESA = '" + reader["COD_EMPRESA"].ToString().Trim() + "' AND");

                                ConsultaDelete.AppendLine("      SEQ_GENERACION = '" + reader["SEQ_GENERACION"].ToString().Trim() + "'");

 

                                OracleCommandDelete.CommandTimeout = 0;

                                OracleCommandDelete.CommandType = CommandType.Text;

                                OracleCommandDelete.CommandText = ConsultaDelete.ToString();

 

                                OracleCommandDelete.ExecuteNonQuery();

                            }

                        }

                    }

                }

 

                foreach (var item in DatosEnvioCuotaSysde.DiasValorCuota)

                {

                    using (var OracleCommand = OracleConnection.CreateCommand())

                    {

                        var ConsultaUpdate = new StringBuilder();

                        ConsultaUpdate.AppendLine("UPDATE PP_FO_VALORES");

                        ConsultaUpdate.AppendLine("     SET VALOR = 0,");

                        ConsultaUpdate.AppendLine("         VALOR_CUOTA_BRUTO = 0,");

                        ConsultaUpdate.AppendLine("         IND_AUTORIZADO = 'N'");

                        ConsultaUpdate.AppendLine("WHERE  COD_INVERSION = '" + item.CodigoFondo + "' AND");

                        ConsultaUpdate.AppendLine("       TO_CHAR(FEC_VALOR,'YYYYMMDD') = '" + item.IdFecha.ToString() + "'");

 

                        OracleCommand.CommandTimeout = 0;

                        OracleCommand.CommandType = CommandType.Text;

                        OracleCommand.CommandText = ConsultaUpdate.ToString();

                        OracleCommand.ExecuteNonQuery();

                    }

 

                    using (var OracleCommand = OracleConnection.CreateCommand())

                    {

                        var ConsultaUpdate = new StringBuilder();

                        ConsultaUpdate.AppendLine("DELETE CLIFAC");

                        ConsultaUpdate.AppendLine("WHERE  FAC_TIP_FDO = '" + item.CodigoFondo + "' AND");

                        ConsultaUpdate.AppendLine("       FAC_FEC_REN = '" + item.IdFecha.ToString() + "'");

 

                        OracleCommand.CommandTimeout = 0;

                        OracleCommand.CommandType = CommandType.Text;

                        OracleCommand.CommandText = ConsultaUpdate.ToString();

                        OracleCommand.ExecuteNonQuery();

                    }

                }

 

                SqlTransaction.Commit();

                OracleTransaction.Commit();

            }

            catch (Exception ex)

            {

                if (SqlTransaction != null)

                    SqlTransaction.Rollback();

 

                if (OracleTransaction != null)

                    OracleTransaction.Rollback();

 

                throw ex;

            }

            finally

            {

                if (SqlTransaction != null)

                    SqlTransaction.Dispose();

 

                if (OracleTransaction != null)

                    OracleTransaction.Dispose();

 

                if (OracleConnection != null)

                    if (OracleConnection.State == ConnectionState.Open)

                        OracleConnection.Close();

 

                if (SqlConnection != null)

                    if (SqlConnection.State == ConnectionState.Open)

                        SqlConnection.Close();

 

                if (SqlConnection != null)

                    SqlConnection.Dispose();

 

                if (OracleConnection != null)

                    OracleConnection.Dispose();

 

            }

        }

 

        public int CerrarIDI(CierreIdiParams parametros)

        {

 

            {

                List<SqlParameter> listaParametros = new List<SqlParameter>();

                listaParametros.Add(new SqlParameter { ParameterName = "@IdFondo", Value = parametros.IdFondo, SqlDbType = SqlDbType.Int });

                listaParametros.Add(new SqlParameter { ParameterName = "@IdFecha", Value = Helper.ConvertFechaStringToIdFecha(parametros.FechaProceso), SqlDbType = SqlDbType.Int });

                listaParametros.Add(new SqlParameter { ParameterName = "@LoginActualizacion", Value = parametros.Login, SqlDbType = SqlDbType.VarChar });

                listaParametros.Add(new SqlParameter { ParameterName = "@UsuarioActualizacion", Value = Constants.UserSystem, SqlDbType = SqlDbType.VarChar });

 

                SqlParameter IdCierreIdi = new SqlParameter { ParameterName = "@IdCierreIdi", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };

                listaParametros.Add(IdCierreIdi);

                _dapperDataAccess.SetCommandTimeout(0);

                _dapperDataAccess.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "EXEC FMS.USP_CERRAR_IDI @IdFondo, @IdFecha, @LoginActualizacion, @UsuarioActualizacion, @IdCierreIdi out", listaParametros.ToArray());

                return Convert.ToInt32(IdCierreIdi.Value);

            }

        }

 

        public CierreIdiConsulta ObtenerDetalleIdiCerrado(int IdFondo, int IdFecha)

        {

            List<SqlParameter> listaParametros = new List<SqlParameter>();

            listaParametros.Add(new SqlParameter { ParameterName = "@IdFondo", Value = IdFondo, SqlDbType = SqlDbType.Int });

            listaParametros.Add(new SqlParameter { ParameterName = "@IdFecha", Value = IdFecha, SqlDbType = SqlDbType.Int });

            return _dapperDataAccess.SqlQueryMultiple<CierreIdiConsulta>("EXEC FMS.USP_ObtenerDetalleIDICerrado @IdFondo, @IdFecha", listaParametros.ToArray(), (reader) =>

            {

                var result = reader.ReadFirstOrDefault<CierreIdiConsulta>();

                if (result != null)

                {

                    result.Validaciones = reader.Read<ValidacionesCierreIdiViewModels>().ToList();

                    result.SubProcesos = reader.Read<SubProcesosCierreIdiViewModels>().ToList();

 

                    #region ControlCruzado

                    var Grupos = reader.Read<GrupoControlCruzadoViewModel>();

                    var Detalle = reader.Read<DetalleControlCruzadoViewModel>();

                    var Diferencias = reader.Read<DiferenciaControlCruzadoViewModel>();

 

                    foreach (var item in Grupos)

                    {

                        item.Detalle = Detalle.Where(x => x.CodigoGrupo.Equals(item.CodigoGrupo)).ToList();

                        item.Diferencia = Diferencias.Where(x => x.CodigoGrupo.Equals(item.CodigoGrupo)).ToList();

                    }

 

                    result.ControlCruzado = Grupos.ToArray();

                    #endregion

 

                    result.StockCxC = reader.ReadFirstOrDefault<CierreIdiStockCC_CP>();

                    result.StockCxP = reader.ReadFirstOrDefault<CierreIdiStockCC_CP>();

 

                    result.EncajeDiario = reader.ReadFirstOrDefault<CierreIdiCalculoEncaje>();

 

                    result.OperacionesBeneficios = reader.ReadFirstOrDefault<OperacionesBeneficiosCierreIdiViewModel>();

                }

                return result;

            });

        }

 

        public void ReabrirIdi(int idCierreIDI, string Login, string Usuario)

        {

            var listaParametros = new List<SqlParameter>();

            listaParametros.Add(new SqlParameter { ParameterName = "@IdCierreIDI", Value = idCierreIDI, SqlDbType = SqlDbType.Int });

            listaParametros.Add(new SqlParameter { ParameterName = "@LoginActualizacion", Value = Login, SqlDbType = SqlDbType.VarChar });

            listaParametros.Add(new SqlParameter { ParameterName = "@UsuarioActualizacion", Value = Usuario, SqlDbType = SqlDbType.VarChar });

            _dapperDataAccess.ExecuteSqlCommand("exec IDI.ReabrirCierreIDI @IdCierreIDI,@LoginActualizacion,@UsuarioActualizacion", listaParametros.ToArray());

        }

 

        public void ValidarForward(int? IdFondo, int IdFecha, string loginActualizacion)

        //PSV(ZX) - 09.08.2021 - REQ. 5863

        {

            List<SqlParameter> listaParametros = new List<SqlParameter>();

            listaParametros.Add(new SqlParameter { ParameterName = "@IdFecha", Value = IdFecha, SqlDbType = SqlDbType.Int });

            listaParametros.Add(new SqlParameter { ParameterName = "@IdFondo", Value = Helper.ValidateNullValue(IdFondo), SqlDbType = SqlDbType.VarChar });

            listaParametros.Add(new SqlParameter { ParameterName = "@LoginActualizacion", Value = loginActualizacion, SqlDbType = SqlDbType.VarChar });

            _dapperDataAccess.ExecuteSqlCommand("EXEC FMS.USP_Validar_Forward_NDF @IdFecha, @IdFondo, @LoginActualizacion", listaParametros.ToArray());

        }

 

 

    }

 

}