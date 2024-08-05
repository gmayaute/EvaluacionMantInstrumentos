namespace FMS.WebAPI.Controllers.MesaDinero
{
    #region References

    using System;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Application.DTO.MesaDinero.Instrumento;
    using Application.Implementation.MesaDinero.InstrumentoAppService;

    #endregion

    [RoutePrefix("api/instrumentoAccion")]
    public class InstrumentoAccionController : ApiController
    {
        readonly IInstrumentoAppService iInstrumentoAppService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentoAppService"></param>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        public InstrumentoAccionController(IInstrumentoAppService instrumentoAppService)
        {
            if (instrumentoAppService == null)
                throw new ArgumentNullException("instrumentoAppService");

            iInstrumentoAppService = instrumentoAppService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentoAccionesDTO"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpPost]
        [Route("AddNewInstrumentoAccion")]
        public IHttpActionResult AddNewInstrumentoAccion([FromBody] InstrumentoAccionDTO instrumentoAccionesDTO)
        {
            return Ok(iInstrumentoAppService.AddNewInstrumentoAccion(instrumentoAccionesDTO));
        }

        [HttpGet]
        [Route("GetAllInstrumentosAcciones/{idFondo}/{idInstrumento}/{codigoIsin}/{idEmisor}/{codigoSbs}/{nemotecnico}")]
        [ResponseType(typeof(InstrumentoListadoFondoDTO[]))]
        public IHttpActionResult GetAllInstrumentosAcciones(int idFondo, int idInstrumento, string codigoIsin, int idEmisor, string codigoSbs, string nemotecnico)
        {
            return Ok(iInstrumentoAppService.GetAllInstrumentosAcciones(idFondo, idInstrumento, codigoIsin, idEmisor, codigoSbs, nemotecnico));
        }

        [HttpGet]
        [Route("GetAllInstrumentosAccionLiberadaAjuste")]
        [ResponseType(typeof(InstrumentoAcuerdoListadoDTO[]))]
        public IHttpActionResult GetAllInstrumentosAccionLiberadaAjuste([FromUri] InstrumentoAcuerdoListadoFilterDTO filter)
        {
            return Ok(iInstrumentoAppService.GetAllInstrumentosAccionLiberadaAjuste(filter.IdInstrumento, filter.IdEmisor, filter.Fechaentrega));
        }

        [HttpGet]
        [Route("GetAll")]
        [ResponseType(typeof(InstrumentoAccionDTO[]))]
        public IHttpActionResult GetAllInstrumentosAcciones()
        {
            return Ok(iInstrumentoAppService.GetAllInstrumentosAcciones());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idInstrumento"></param>
        /// <param name="idAccion"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpGet]
        [Route("GetByIdInstrumentoAccion/{idInstrumento}/{idAccion}")]
        [ResponseType(typeof(InstrumentoAccionDTO))]
        public IHttpActionResult GetByIdInstrumentoAccion(int idInstrumento, int idAccion)
        {
            return Ok(iInstrumentoAppService.GetByIdInstrumentoAccion(idInstrumento, idAccion));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idInstrumento"></param>
        /// <param name="idAccion"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpGet]
        [Route("GetAllAccionByActiveRiesgo/{grupo}")]
        [ResponseType(typeof(InstrumentoAccionDTO[]))]
        public IHttpActionResult GetAllAccionByActiveRiesgo(string grupo)
        {
            return Ok(iInstrumentoAppService.GetAllAccionByActiveRiesgo(grupo));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentoAccionesDTO"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpPut]
        [Route("UpdateInstrumentoAccion")]
        public IHttpActionResult UpdateInstrumentoAccion([FromBody] InstrumentoAccionDTO instrumentoAccionesDTO)
        {
            return Ok(iInstrumentoAppService.UpdateInstrumentoAccion(instrumentoAccionesDTO));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentoAccionesDTO"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpPut]
        [Route("AnnulInstrumentoAccion")]
        public IHttpActionResult AnnulInstrumentoAccion([FromBody] InstrumentoAccionDTO instrumentoAccionesDTO)
        {
            return Ok(iInstrumentoAppService.AnnulInstrumentoAccion(instrumentoAccionesDTO));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrumentoAccionesDTO"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpPut]
        [Route("ActiveInstrumentoAccion")]
        public IHttpActionResult ActiveInstrumentoAccion([FromBody] InstrumentoAccionDTO instrumentoAccionesDTO)
        {
            return Ok(iInstrumentoAppService.ActiveInstrumentoAccion(instrumentoAccionesDTO));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idInstrumento"></param>
        /// <param name="idAccion"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpDelete]
        [Route("RemoveInstrumentoAccion/{idInstrumento}/{idAccion}")]
        public IHttpActionResult RemoveInstrumentoAccion(int idInstrumento, int idAccion)
        {
            return Ok(iInstrumentoAppService.RemoveInstrumentoAccion(idInstrumento, idAccion));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codMoneda"></param>
        /// <param name="descripcionCorta"></param>
        /// <param name="idNacionlaidad"></param>
        /// <param name="idPais"></param>
        /// <param name="idMondeCambio"></param>
        /// <param name="idRelacionCambio"></param>
        /// <param name="indActividad"></param>
        /// <param name="indHabilitacion"></param>
        /// <param name="currentIndexPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="columnName"></param>
        /// <param name="isAscending"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpGet]
        [Route("GetFilteredDataAcciones/{codigoSbs}/{codigoIsin}/{tipoAccion}/{idEmisor}/{idMoneda}/{indActividad}/{indHabilitacion}/{idInstrumento}/{currentIndexPage}/{itemsPerPage}/{columnName}/{isAscending}")]
        [ResponseType(typeof(InstrumentoAccionPagedDTO))]
        public IHttpActionResult GetFilteredDataAcciones(string codigoSbs, string codigoIsin, int tipoAccion, int idEmisor, int idMoneda, int indActividad, int indHabilitacion, int idInstrumento, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            return Ok(iInstrumentoAppService.GetFilteredDataAcciones(codigoSbs, codigoIsin, tipoAccion, idEmisor, idMoneda, indActividad, indHabilitacion, idInstrumento, currentIndexPage, itemsPerPage, columnName, isAscending));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codMoneda"></param>
        /// <param name="descripcionCorta"></param>
        /// <param name="idNacionlaidad"></param>
        /// <param name="idPais"></param>
        /// <param name="idMondeCambio"></param>
        /// <param name="idRelacionCambio"></param>
        /// <param name="indActividad"></param>
        /// <param name="indHabilitacion"></param>
        /// <param name="currentIndexPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="columnName"></param>
        /// <param name="isAscending"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created: GYS(JAS) 20160315
        /// Modified: GYS(JAS) 20160315
        /// </remarks>
        [HttpGet]
        [Route("GetInstrumentoGrupoFilteredData/{currentIndexPage}/{itemsPerPage}/{columnName}/{isAscending}/{idGrupoInstrumento}/{idMandato}")]
        [ResponseType(typeof(InstrumentoGrupoInstrumentoPagedDTO))]
        public IHttpActionResult GetInstrumentoGrupoFilteredData(int currentIndexPage, int itemsPerPage, string columnName, bool isAscending, int idGrupoInstrumento, int idMandato)
        {
            return Ok(iInstrumentoAppService.GetInstrumentoGrupoFilteredData(currentIndexPage, itemsPerPage, columnName, isAscending, idGrupoInstrumento, idMandato));
        }
    }
}