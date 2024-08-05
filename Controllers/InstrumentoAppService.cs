namespace FMS.Application.Implementation.MesaDinero.InstrumentoAppService
{
    #region References

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Transactions;
    using Administracion.CalendarioAppService;
    using Administracion.DiasNoLaborablesAppService;
    using Administracion.GrupoInstrumentoAppService;
    using Base;
    using Domain.Implementation.Custodia.Aggregates.CriterioCalculoRebateAgg;
    using Domain.Implementation.MesaDinero.Aggregates.EntidadAgg;
    using Domain.Implementation.MesaDinero.Aggregates.InstrumentoAgg;
    using Domain.Implementation.MesaDinero.Aggregates.OrdenInversionAgg;
    using Domain.Implementation.MesaDinero.Aggregates.TipoInstrumentoAgg;
    using Domain.Implementation.MesaDinero.Aggregates.TipoOperacionAgg;
    using Domain.Implementation.Shared.Aggregates.IndicadorAgg;
    using DTO.Administracion.GrupoInstrumento;
    using DTO.Custodia.AccionesLiberadas;
    using DTO.Custodia.InteresesDevengados;
    using DTO.MesaDinero.Instrumento;
    using DTO.MesaDinero.Moneda;
    using DTO.MesaDinero.TipoInstrumento;
    using DTO.MesaDinero.TipoOperacionCapitalCash;
    using DTO.Shared;
    using FMS.Shared.Constants;
    using FMS.Shared.Enum;
    using FMS.Shared.Helper;
    using IdiAppService;
    using IndiceIndexacionMonedaAppService;
    using Infrastructure.CrossCutting.Base.Validator;
    using Infrastructure.Data.Implementation.IDataAccess.Repositories.MesaDinero;
    using MonedaAppService;
    using Resources.mensajeValidacion;
    using Shared.IndicadorAppService;
    using Tesoreria.TasaAppService;
    using TipoInstrumentoAppService;
    using VACAppService;
    using DTO.MesaDinero.Idi;
    using FMS.Domain.Implementation.MesaDinero.Aggregates.MonedaAgg;
    using FMS.Application.Implementation.MesaDinero.EntidadAppService;
    using Domain.Implementation.Contabilidad.Aggregates.AnexosAgg;
    using FMS.Domain.Implementation.Custodia.Aggregates.FondoAlternativoAgg;
    using FMS.Application.DTO.Custodia.FondoAlternativoActualizacionPrecio;

    //using Domain.Implementation.MesaDinero.Aggregates.TipoOperacionCapitalCashAgg;

    #endregion

    public class InstrumentoAppService : IInstrumentoAppService
    {
        #region Members

        readonly IFechaAppService iFechaAppService;
        readonly IInstrumentoRepository iInstrumentoRepository;
        readonly IInstrumentoAccionRepository iInstrumentoAccionRepository;
        readonly IInstrumentoCertificadoSuscripcionPreferenteRepository iInstrumentoCertificadoSuscripcionPreferenteRepository;
        readonly IInstrumentoFondoMutuoRepository iInstrumentoFondoMutuoRepository;
        readonly IInstrumentoNotaEstructuradaRepository iInstrumentoNotaEstructuradaRepository;
        readonly IInstrumentoFondoAlternativoRepository iInstrumentoFondoAlternativoRepository;
        readonly IInstrumentoRentaFijaRepository iInstrumentoRentaFijaRepository;
        readonly IInstrumentoRentaFijaCuponRepository iInstrumentoRentaFijaCuponRepository;
        readonly IInstrumentoOpcionRepository iInstrumentoOpcionRepository;
        readonly IInstrumentoFuturoRepository iInstrumentoFuturoRepository;
        readonly IInstrumentoVectorPrecioSbsRepository iInstrumentoVectorPrecioSbsRepository;
        readonly IInstrumentoVectorPrecioCargaSbsRepository iInstrumentoVectorPrecioCargaSbsRepository;
        readonly IInstrumentoCertificadoDepositoCortoPlazoRepository iInstrumentoCertificadoDepositoCortoPlazoRepository;
        readonly IInstrumentoCertificadoDepositoCortoPlazoCuponRepository iInstrumentoCertificadoDepositoCortoPlazoCuponRepository;
        readonly IIndicadorAppService iIndicadorAppService;
        readonly ITipoInstrumentoAppService iTipoInstrumentoAppService;
        readonly ITipoInstrumentoRepository iTipoInstrumentoRepository;
        readonly IInstrumentoDataRepository iInstrumentoDataRepository;
        readonly IGrupoInstrumentoAppService iGrupoInstrumentoAppService;
        readonly IInstrumentoFondoAlternativoTasaRepository iFondoAlternativoTasaRepository;
        readonly IInstrumentoFondoAlternativoComprometidoRepository iFondoAlternativoComprometidoRepository;
        readonly IInstrumentoFondoAlternativoLlamadaRepository iFondoAlternativoLlamadaRepository;
        readonly IInstrumentoFondoAlternativoDetalleComprometidoRepository iFondoAlternativoDetalleComprometidoRepository;
        readonly IInstrumentoFondoAlternativoDetalleLlamadaRepository iFondoAlternativoDetalleLlamadaRepository;
        readonly IIndicadorRepository iIndicadorRepository;
        readonly IDiasNoLaborablesAppService iDiasNoLaborablesAppService;
        readonly IVacAppService iVacAppService;
        readonly ITasaAppService iTasaAppService;
        readonly IEntidadRepository iEntidadRepository;
        readonly IMonedaAppService iMonedaAppService;
        readonly IEntidadAppService iEntidadAppService;
        readonly IIndiceIndexacionMonedaAppService iIndiceIndexacionMonedaAppService;
        readonly ICriterioCalculoRebateRepository iCriterioCalculoRebateRepository;
        readonly IInstrumentoStockRepository iInstrumentoStockRepository;
        readonly IIdiAppService iIdiAppService;
        readonly IOrdenInversionRepository iOrdenInversionRepository;
        readonly IVariacionCodigoSBSRepository iVariacionCodigoSBSRepository;
        readonly IVariacionIsinRepository iVariacionIsinRepository;
        readonly IAnexoIRubroRepository iAnexoIRubroRepository;
        readonly IFondoAlternativoActualizacionPrecioRepository iFondoAlternativoActualizacionPrecioRepository;
        //readonly ITipoOperacionCapitalCashRepository iTipoOperacionCapitalCashRepository;

        TransactionOptions transactionScopeOptions = new TransactionOptions();

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grupoInstrumentoRepository"></param>
        /// <param name="indicadorAppService"></param>
        public InstrumentoAppService(IInstrumentoRepository instrumentoRepository, IInstrumentoAccionRepository instrumentoAccionRepository,
            IInstrumentoCertificadoSuscripcionPreferenteRepository instrumentoCertificadoSuscripcionPreferenteRepository,
            IInstrumentoFondoMutuoRepository instrumentoFondoMutuoRepository, IInstrumentoNotaEstructuradaRepository instrumentoNotaEstructuradaRepository,
            IInstrumentoOpcionRepository instrumentoOpcionRepository, IInstrumentoFuturoRepository instrumentoFuturoRepository,
            IInstrumentoRentaFijaRepository instrumentoRentaFijaRepository, IInstrumentoRentaFijaCuponRepository instrumentoRentaFijaCuponRepository,
            IInstrumentoFondoAlternativoRepository instrumentoFondoAlternativoRepository,
            IInstrumentoVectorPrecioSbsRepository instrumentoVectorPrecioSbsRepository,
            IInstrumentoVectorPrecioCargaSbsRepository instrumentoVectorPrecioCargaSbsRepository,
            IInstrumentoCertificadoDepositoCortoPlazoRepository instrumentoCertificadoDepositoRepository,
            IInstrumentoCertificadoDepositoCortoPlazoCuponRepository instrumentoCertificadoDepositoCuponRepository,
            IIndicadorAppService indicadorAppService, IInstrumentoDataRepository instrumentoDataRepository, IFechaAppService fechaAppService,
            ITipoInstrumentoAppService tipoInstrumentoAppService, IGrupoInstrumentoAppService grupoInstrumentoAppService,
            IInstrumentoFondoAlternativoTasaRepository fondoAlternativoTasaRepository,
            IInstrumentoFondoAlternativoComprometidoRepository fondoAlternativoComprometidoRepository,
            IInstrumentoFondoAlternativoLlamadaRepository fondoAlternativoLlamadaRepository,
            IInstrumentoFondoAlternativoDetalleComprometidoRepository fondoAlternativoDetalleComprometidoRepository,
            IInstrumentoFondoAlternativoDetalleLlamadaRepository fondoAlternativoDetalleLlamadaRepository,
            IIndicadorRepository indicadorRepository, IDiasNoLaborablesAppService diasNoLaborablesAppService,
            IVacAppService vacAppService, ITasaAppService tasaAppService,
            ICriterioCalculoRebateRepository criterioCalculoRebateRepository,

            IInstrumentoStockRepository instrumentoStockRepository,
            IIdiAppService iIdiAppService,
            IEntidadRepository entidadRepository,
            IMonedaAppService monedaAppService,
            IIndiceIndexacionMonedaAppService indiceIndexacionMonedaAppService,
            ITipoInstrumentoRepository iTipoInstrumentoRepository,
            IOrdenInversionRepository ordenInversionRepository,
            IVariacionCodigoSBSRepository variacionCodigoSBSRepository,
            IVariacionIsinRepository variacionIsinRepository,
            IEntidadAppService entidadAppService,
            IAnexoIRubroRepository AnexoIRubroRepository,
            IFondoAlternativoActualizacionPrecioRepository iFondoAlternativoActualizacionPrecioRepository
            //ITipoOperacionCapitalCashRepository iTipoOperacionCapitalCashRepository
            )
        {
            if (variacionIsinRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "variacionIsinRepository"));

            if (variacionCodigoSBSRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "variacionCodigoSBSRepository"));

            if (ordenInversionRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "ordenInversionRepository"));

            if (instrumentoRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoRepository"));

            if (indicadorAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "indicadorAppService"));

            if (instrumentoDataRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoDataRepository"));

            if (instrumentoAccionRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoAccionRepository"));

            if (instrumentoCertificadoSuscripcionPreferenteRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoCertificadoSuscripcionPreferenteRepository"));

            if (instrumentoFondoMutuoRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoMutuoRepository"));

            if (instrumentoNotaEstructuradaRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoNotaEstructuradaRepository"));

            if (instrumentoRentaFijaRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoRentaFijaRepository"));

            if (instrumentoRentaFijaCuponRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoRentaFijaCuponRepository"));

            if (instrumentoOpcionRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoOpcionRepository"));

            if (instrumentoFuturoRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFuturoRepository"));

            if (instrumentoFondoAlternativoRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoRepository"));

            if (instrumentoVectorPrecioSbsRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoVectorPrecioSbsRepository"));

            if (instrumentoVectorPrecioCargaSbsRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoVectorPrecioCargaSbsRepository"));

            if (instrumentoCertificadoDepositoRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoCertificadoDepositoRepository"));

            if (instrumentoCertificadoDepositoCuponRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoCertificadoDepositoCuponRepository"));

            if (fechaAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fechaAppService"));

            if (tipoInstrumentoAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "tipoInstrumentoAppService"));

            if (grupoInstrumentoAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "grupoInstrumentoAppService"));

            if (fondoAlternativoTasaRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoTasaRepository"));

            if (fondoAlternativoComprometidoRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoComprometidoRepository"));

            if (fondoAlternativoLlamadaRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoLlamadaRepository"));

            if (fondoAlternativoDetalleComprometidoRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoDetalleComprometidoRepository"));

            if (fondoAlternativoDetalleLlamadaRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoDetalleLlamadaRepository"));

            if (indicadorRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "indicadorRepository"));

            if (diasNoLaborablesAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "calendarioExcepcionesRepository"));

            if (vacAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "vacAppService"));

            if (tasaAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "tasaAppService"));

            if (monedaAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "monedaAppService"));

            if (criterioCalculoRebateRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "criterioCalculoRebateRepository"));

            if (indiceIndexacionMonedaAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "indiceIndexacionMonedaAppService"));

            if (instrumentoStockRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoStockRepository"));

            if (iIdiAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "iIdiAppService"));

            if (iTipoInstrumentoRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "iTipoInstrumentoRepository"));

            if (entidadAppService == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "iEntidadAppService"));
            if (AnexoIRubroRepository == null)
                throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "AnexoIRubroRepository"));

            //if (iTipoOperacionCapitalCashRepository == null)
            //    throw new ArgumentNullException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "tipoOperacionCapitalCashRepository"));

            transactionScopeOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            transactionScopeOptions.Timeout = TimeSpan.MaxValue;

            iInstrumentoRepository = instrumentoRepository;
            iIndicadorAppService = indicadorAppService;
            iInstrumentoDataRepository = instrumentoDataRepository;
            iInstrumentoAccionRepository = instrumentoAccionRepository;
            iInstrumentoCertificadoSuscripcionPreferenteRepository = instrumentoCertificadoSuscripcionPreferenteRepository;
            iInstrumentoFondoMutuoRepository = instrumentoFondoMutuoRepository;
            iInstrumentoNotaEstructuradaRepository = instrumentoNotaEstructuradaRepository;
            iInstrumentoRentaFijaRepository = instrumentoRentaFijaRepository;
            iInstrumentoFondoAlternativoRepository = instrumentoFondoAlternativoRepository;
            iInstrumentoRentaFijaCuponRepository = instrumentoRentaFijaCuponRepository;
            iInstrumentoOpcionRepository = instrumentoOpcionRepository;
            iInstrumentoFuturoRepository = instrumentoFuturoRepository;
            iInstrumentoVectorPrecioSbsRepository = instrumentoVectorPrecioSbsRepository;
            iInstrumentoVectorPrecioCargaSbsRepository = instrumentoVectorPrecioCargaSbsRepository;
            iInstrumentoCertificadoDepositoCortoPlazoRepository = instrumentoCertificadoDepositoRepository;
            iInstrumentoCertificadoDepositoCortoPlazoCuponRepository = instrumentoCertificadoDepositoCuponRepository;
            iFechaAppService = fechaAppService;
            iTipoInstrumentoAppService = tipoInstrumentoAppService;
            iGrupoInstrumentoAppService = grupoInstrumentoAppService;
            iFondoAlternativoTasaRepository = fondoAlternativoTasaRepository;
            iFondoAlternativoComprometidoRepository = fondoAlternativoComprometidoRepository;
            iFondoAlternativoLlamadaRepository = fondoAlternativoLlamadaRepository;
            iFondoAlternativoDetalleComprometidoRepository = fondoAlternativoDetalleComprometidoRepository;
            iFondoAlternativoDetalleLlamadaRepository = fondoAlternativoDetalleLlamadaRepository;
            iIndicadorRepository = indicadorRepository;
            iDiasNoLaborablesAppService = diasNoLaborablesAppService;
            iVacAppService = vacAppService;
            iTasaAppService = tasaAppService;
            iEntidadRepository = entidadRepository;
            iMonedaAppService = monedaAppService;
            iCriterioCalculoRebateRepository = criterioCalculoRebateRepository;
            iIndiceIndexacionMonedaAppService = indiceIndexacionMonedaAppService;
            iInstrumentoStockRepository = instrumentoStockRepository;
            this.iIdiAppService = iIdiAppService;

            this.iTipoInstrumentoRepository = iTipoInstrumentoRepository;
            this.iOrdenInversionRepository = ordenInversionRepository;
            this.iVariacionCodigoSBSRepository = variacionCodigoSBSRepository;
            this.iVariacionIsinRepository = variacionIsinRepository;
            this.iEntidadAppService = entidadAppService;
            this.iAnexoIRubroRepository = AnexoIRubroRepository;
            this.iFondoAlternativoActualizacionPrecioRepository = iFondoAlternativoActualizacionPrecioRepository;
            //this.iTipoOperacionCapitalCashRepository = iTipoOperacionCapitalCashRepository;
        }

        #endregion

        #region Instrumento Members

        public List<InstrumentoOrdenInversionDTO> GetAllInstrumentoInEventoCorporativo()
        {
            return iInstrumentoDataRepository.GetAllInstrumentoInEventoCorporativo();
        }

        public InstrumentoVectorPrecioSbsDTO GetPrecioVectorByInstrumentoAndFechaOperacion(int idInstrumento, string fechaOperacion)
        {
            int idActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            //int idFechaOrden = iFechaRepository.GetFiltered(x => x.Fecha1 == Convert.ToDateTime(fechaOperacion)).FirstOrDefault(;
            int idFechaOperacion = Helper.ConvertFechaStringToIdFecha(fechaOperacion);// (fechaOperacion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fechaOperacion);

            if (idFechaOperacion == 0 || idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);
            if (instrumento == null)
                throw new Exception(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);
            var precio = iInstrumentoVectorPrecioSbsRepository.GetFiltered(x => x.IdFechaInstrumentoPrecio == idFechaOperacion
            && x.IdInstrumento == instrumento.IdInstrumento
            && x.IdMoneda == instrumento.IdMoneda
            && x.IndActividad == idActividad).FirstOrDefault();

            return precio.ProjectedAs<InstrumentoVectorPrecioSbsDTO>();
        }

        public InstrumentoOrdenInversionDTO GetInstrumentoByIdInstrumento(int idInstrumento)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);
            return iInstrumentoDataRepository.GetInstrumentoByIdInstrumento(idInstrumento);
        }
        /// <summary>
        /// This method return all the instrumentos by idTipoInstrumento. Send zero to get all the instrumentos
        /// </summary>
        /// <param name="idTipoInstrumento"></param>
        /// <returns></returns>
        public List<InstrumentoDTO> GetAllInstrumentoByIdTipoInstrumento(int idTipoInstrumento)
        {
            List<Instrumento> list;
            if (idTipoInstrumento == 0)
                list = iInstrumentoRepository.GetAll().ToList();
            else
                list = iInstrumentoRepository.GetFiltered(x => (x.IdTipoInstrumento == idTipoInstrumento)).ToList();
            return list.ProjectedAsCollection<InstrumentoDTO>();
        }

        public List<InstrumentoBusquedaDTO> GetAllInstrumentoByNombreInstrumento(string nombreInstrumento, int totalRecords)
        {
            List<InstrumentoBusquedaDTO> query;
            if (totalRecords != 0)
                query = iInstrumentoDataRepository.GetAllInstrumentoByNombreInstrumento(nombreInstrumento).Take(totalRecords).ToList();
            else
                query = iInstrumentoDataRepository.GetAllInstrumentoByNombreInstrumento(nombreInstrumento);
            return query;
        }

        //public List<InstrumentoBusquedaDTO> GetAllInstrumentoByNombreInstrumento(string nombreInstrumento, int totalRecords)
        //{
        //    List<InstrumentoBusquedaDTO> query;
        //    if (totalRecords != 0)
        //        query = iInstrumentoDataRepository.GetAllInstrumentoByNombreInstrumento(nombreInstrumento).Take(totalRecords).ToList();
        //    else
        //        query = iInstrumentoDataRepository.GetAllInstrumentoByNombreInstrumento(nombreInstrumento);
        //    return query;
        //}

        public List<InstrumentoOrdenInversionDTO> GetAllInstrumentoRentaFija()
        {
            return iInstrumentoDataRepository.GetAllInstrumentoRentaFija();
        }

        public List<InstrumentoOrdenInversionDTO> GetAllActiveInstrumentosByIdTipoInstrumentoAndIdEmisor(int idTipoInstrumento, int idEmisor)
        {
            return iInstrumentoDataRepository.GetAllActiveInstrumentosByIdTipoInstrumentoAndIdEmisor(idTipoInstrumento, idEmisor);
        }

        public InstrumentoOrdenInversionDTO getInstrumentosById(int idInstrumento)
        {
            return iInstrumentoDataRepository.GetInstrumentosById(idInstrumento);
        }


        /// <summary>
        /// This method verifies if the Entidad has instrumentos related
        /// </summary>
        /// <returns></returns>
        public bool IdEmisorHasInstrumentosRelated(int idEntidad)
        {
            int existingsInstrumento = iInstrumentoRepository.GetFiltered(x => (x.IdEmisor != idEntidad)).Count();
            return (existingsInstrumento > 0) ? true : false;
        }

        public InstrumentoPagedDTO GetFilteredData(string codigoSbs, int indActividad, int indHabilitacion, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;

            InstrumentoPagedDTO response = iInstrumentoDataRepository.GetFilteredData(codigoSbs, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);
            foreach (InstrumentoListadoDTO item in response.ListaInstrumento)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }
            return response;
        }

        public List<InstrumentoDTO> GetAllInstrumentoByGrupoInstrumentoAndEmisorNacional(string codigoGrupoInstrumento)
        {
            return iInstrumentoRepository.GetFiltered(i => i.GrupoInstrumento.Grupo.Equals(codigoGrupoInstrumento, StringComparison.OrdinalIgnoreCase) &&
                                                           i.Emisor.Pais.ValorAuxChar1.Equals("Nacional", StringComparison.OrdinalIgnoreCase))
                                         .ProjectedAsCollection<InstrumentoDTO>();
        }

        public List<InstrumentoDTO> GetAllActiveInstrumentoByGrupoInstrumentoAndEmisorNacional(string codigoGrupoInstrumento)
        {
            int idActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            return iInstrumentoRepository.GetFiltered(i => i.GrupoInstrumento.Grupo.Equals(codigoGrupoInstrumento, StringComparison.OrdinalIgnoreCase) &&
                                                           i.Emisor.Pais.ValorAuxChar1.Equals("Nacional", StringComparison.OrdinalIgnoreCase) &&
                                                           i.IndActividad == idActividad)
                                         .ProjectedAsCollection<InstrumentoDTO>();
        }

        public List<InstrumentoListadoFondoDTO> GetAllInstrumentosByFondoAndNemotecnicoAndEmisorAndIsinAndSbs(int idFondo, int idInstrumento, string codigoIsin, int idEmisor, string codigoSbs)
        {

            codigoIsin = codigoIsin.Equals("_") ? "" : codigoIsin;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;

            return iInstrumentoDataRepository.GetAllInstrumentosByFondoAndNemotecnicoAndEmisorAndIsinAndSbs(idFondo, idInstrumento, codigoIsin, idEmisor, codigoSbs).ToList();
        }

        public List<InstrumentoListadoFondoDTO> GetAllInstrumentosByFondoAndNombreInstrumento(int idFondo, string nombreInstrumento)
        {

            nombreInstrumento = nombreInstrumento.Equals("_") ? "" : nombreInstrumento;

            return iInstrumentoDataRepository.GetAllInstrumentosByFondoAndNombreInstrumento(idFondo, nombreInstrumento).ToList();
        }
        public List<InstrumentoListadoFondoDTO> GetAllInstrumentosAcciones(int idFondo, int idInstrumento, string codigoIsin, int idEmisor, string codigoSbs, string Nemotecnico)
        {
            codigoIsin = codigoIsin.Equals("_") ? "" : codigoIsin;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;
            Nemotecnico = Nemotecnico.Equals("_") ? "" : Nemotecnico;

            return iInstrumentoDataRepository.GetAllInstrumentosAccionLiberadaAcuerdo(idFondo, idInstrumento, codigoIsin, idEmisor, codigoSbs, Nemotecnico).ToList();
        }
        public List<InstrumentoAcuerdoListadoDTO> GetAllInstrumentosAccionLiberadaAjuste(int IdInstrumento, int IdEmisor, string fechaentrega)
        {
            int idFecha = Helper.ConvertFechaStringToIdFecha(fechaentrega);
            return iInstrumentoDataRepository.GetAllInstrumentosAccionLiberadaAjuste(IdInstrumento, IdEmisor, idFecha).ToList();
        }
        public List<AccionLiberadaAjusteEmisorInstrumentoDTO> GetEntidadAcuerdoAjusteBynemotecnico(AccionLiberadaAjusteEmisorInstrumentoFilterDTO filter)
        {
            return iInstrumentoDataRepository.GetEntidadAcuerdoAjusteBynemotecnico(filter).ToList();
        }
        public List<AccionLiberadaAjusteEmisorInstrumentoDTO> GetEntidadAcuerdoBynemotecnico(AccionLiberadaAjusteEmisorInstrumentoFilterDTO filter)
        {
            return iInstrumentoDataRepository.GetEntidadAcuerdoBynemotecnico(filter).ToList();
        }
        public InstrumentoAccionDTO[] GetAllInstrumentosAcciones()
        {
            return iInstrumentoAccionRepository.GetAll().ProjectedAsCollection<InstrumentoAccionDTO>().ToArray();
        }

        public List<InstrumentoListadoListaTenenciaDTO> GetAllInstrumentoForFilterListaTenencias(InstrumentoFilterListaTenenciaDTO instrumentoFilter)
        {
            return iInstrumentoDataRepository.GetAllInstrumentoForFilterListaTenencias(instrumentoFilter);
        }

        public List<InstrumentoListadoListaTenenciaDTO> GetAllInstrumentoForFilterDerechoObligacion(InstrumentoFilterListaTenenciaDTO instrumentoFilter)
        {
            return iInstrumentoDataRepository.GetAllInstrumentoForFilterDerechoObligacion(instrumentoFilter);
        }

        public List<InstrumentoOrdenInversionDTO> GetAllActiveAndEnableInstrumentosByIdTipoInstrumento(int idTipoInstrumento)
        {
            return iInstrumentoDataRepository.GetAllActiveAndEnableInstrumentosByIdTipoInstrumento(idTipoInstrumento);
        }
        public InstrumentoOrdenInversionDTO[] GetInstrumentosOperacionePrestamoValoresByFecha(string FechaInicio, string FechaFin)
        {
            int? IdFechaInicio = null;
            int? IdFechaFin = null;

            if (!string.IsNullOrEmpty(FechaInicio))
                IdFechaInicio = Helper.ConvertFechaStringToIdFecha(FechaInicio);

            if (!string.IsNullOrEmpty(FechaFin))
                IdFechaFin = Helper.ConvertFechaStringToIdFecha(FechaFin);

            return iInstrumentoDataRepository.GetInstrumentosOperacionePrestamoValoresByFecha(IdFechaInicio, IdFechaFin);
        }

        public List<InstrumentoComboDTO> GetAllInstrumentoRentaFijaOrVariableFilterByNotGrupo(string codigoGrupoInstrumento)
        {
            int idActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            int idHabilitacion = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);
            return iInstrumentoDataRepository.GetAllInstrumentoRentaFijaOrVariableFilterByNotGrupo(codigoGrupoInstrumento, true, true);

        }


        #endregion

        #region InstrumentoAccionesAppService Members
        public InstrumentoAccionPagedDTO GetFilteredDataAcciones(string codigoSbs, string codigoIsin, int tipoAccion, int idEmisor, int idMoneda, int indActividad, int indHabilitacion, int idInstrumento, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? string.Empty : columnName;
            codigoSbs = codigoSbs.Equals("_") ? string.Empty : codigoSbs;
            codigoIsin = codigoIsin.Equals("_") ? string.Empty : codigoIsin;

            InstrumentoAccionPagedDTO response = iInstrumentoDataRepository.GetFilteredDataAcciones(codigoSbs, codigoIsin, tipoAccion, idEmisor, idMoneda, indActividad, indHabilitacion, idInstrumento, currentIndexPage, itemsPerPage, columnName, isAscending);
            foreach (InstrumentoAccionListadoDTO item in response.ListaInstrumentoAccion)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }
            return response;
        }
        public InstrumentoGrupoInstrumentoPagedDTO GetInstrumentoGrupoFilteredData(int currentIndexPage, int itemsPerPage, string columnName, bool isAscending, int idGrupoInstrumento, int idMandato)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;

            InstrumentoGrupoInstrumentoPagedDTO response = iInstrumentoDataRepository.GetInstrumentoGrupoFilteredData(currentIndexPage, itemsPerPage, columnName, isAscending, idGrupoInstrumento, idMandato);
            return response;
        }
        public string UpdateInstrumentoAccion(InstrumentoAccionDTO instrumentoAccionesDTO)
        {
            /*using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {*/
            if (instrumentoAccionesDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoAccion"));

            var Instrumento = iInstrumentoRepository.Get(instrumentoAccionesDTO.IdInstrumento);
            var Accion = iInstrumentoAccionRepository.Get(instrumentoAccionesDTO.IdAccion);

            if (Instrumento == null || Accion == null)
                throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

            if (Accion.IsAdrAds != null && Accion.IsAdrAds.Value)
            {
                Accion.MontoEmitido = instrumentoAccionesDTO.MontoEmitido;
                Accion.IdSecuencialFechaMontoEmitido = (instrumentoAccionesDTO.SecuencialFechaMontoEmitido.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaMontoEmitido);
                Accion.MontoColocado = instrumentoAccionesDTO.MontoColocado;
                Accion.IdSecuencialFechaMontoColocado = (instrumentoAccionesDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaMontoColocado);
                Accion.NroUnidadesFloat = instrumentoAccionesDTO.NroUnidadesFloat;
                Instrumento.IdClasificacionRiesgo = instrumentoAccionesDTO.IdClasificacionRiesgo;
                Accion.IndPaisEmisor = instrumentoAccionesDTO.IndPaisEmisor;
            }
            else
            {
                VerifyNombreInstrumentoIsUnique(instrumentoAccionesDTO.CodigoSbs, instrumentoAccionesDTO.IdInstrumento);
                VerifyInstrumentoAccionIsUnique(instrumentoAccionesDTO);

                var InabilitadoNuevo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);
                var EstadoVigente = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                var InabilitadoModificado = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);
                var Habilitado = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);
                var indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoAccionesDTO.IndHabilitacionRiesgo);

                Instrumento.IdTipoInstrumento = instrumentoAccionesDTO.IdTipoInstrumento;
                Instrumento.NombreInstrumento = instrumentoAccionesDTO.NombreInstrumento;
                Instrumento.CodigoSbs = instrumentoAccionesDTO.CodigoSbs;
                Instrumento.IdMoneda = instrumentoAccionesDTO.IdMoneda;
                Instrumento.IdEmisor = instrumentoAccionesDTO.IdEmisor;
                Instrumento.IndCategoria = instrumentoAccionesDTO.IndCategoria;
                Instrumento.IndFamilia = instrumentoAccionesDTO.IndFamilia;
                Instrumento.IdClasificacionRiesgo = instrumentoAccionesDTO.IdClasificacionRiesgo;
                Instrumento.LoginActualizacion = instrumentoAccionesDTO.LoginActualizacion;
                Instrumento.IndHabilitacionRiesgo = indHabilitacionRiesgo;

                Accion.TieneMonedaDual = instrumentoAccionesDTO.TieneMonedaDual;
                Accion.IdMonedaDual = instrumentoAccionesDTO.IdMonedaDual == (int)CustomLength.ErrorValue ? new Nullable<int>() : instrumentoAccionesDTO.IdMonedaDual;
                Accion.IdSecuencialFechaEmision = (instrumentoAccionesDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaEmision);
                Accion.IdSecuencialFechaVencimiento = (instrumentoAccionesDTO.SecuencialFechaVencimiento.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaVencimiento);
                Accion.ValorNominal = instrumentoAccionesDTO.ValorNominal;
                Accion.ValorNominalSbs = instrumentoAccionesDTO.ValorNominalSbs;
                Accion.IndTipoCustodia = instrumentoAccionesDTO.IndTipoCustodia;
                Accion.IndClase = instrumentoAccionesDTO.IndClase;
                Accion.CodIsin = instrumentoAccionesDTO.CodIsin;
                Accion.Nemotecnico = instrumentoAccionesDTO.Nemotecnico;
                Accion.MontoColocado = instrumentoAccionesDTO.MontoColocado;
                Accion.MontoEmitido = instrumentoAccionesDTO.MontoEmitido;
                Accion.IdSecuencialFechaMontoEmitido = (instrumentoAccionesDTO.SecuencialFechaMontoEmitido.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaMontoEmitido);
                Accion.IdSecuencialFechaMontoColocado = (instrumentoAccionesDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaMontoColocado);
                Accion.IndTipoUnidadEmision = instrumentoAccionesDTO.IndTipoUnidadEmision;
                Accion.NroUnidadesFloat = instrumentoAccionesDTO.NroUnidadesFloat;
                Accion.IndPaisEmisor = instrumentoAccionesDTO.IndPaisEmisor;
                Accion.IndFocoGeograficoEmision = instrumentoAccionesDTO.IndFocoGeograficoEmision;
                Accion.IndRegionEmision = instrumentoAccionesDTO.IndRegionEmision;
                Accion.TieneFechaVencimiento = instrumentoAccionesDTO.TieneFechaVencimiento;
                Accion.TieneMandato = instrumentoAccionesDTO.TieneMandato;
                Accion.IsAdrAds = false;
                Accion.LoginActualizacion = instrumentoAccionesDTO.LoginActualizacion;
                Accion.FechaHoraActualizacion = DateTime.Now;
                Accion.UsuarioActualizacion = Constants.UserSystem;

                if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO != null && instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.Any())
                {
                    foreach (var item in instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO)
                    {
                        var codigoSbsGenerated = GeneratedCodigoSbsOnlyFirst7Digits(item.CodigoSbs, item.IdMoneda, instrumentoAccionesDTO.IdEmisor, item.IdTipoInstrumento, 1);
                        VerifyNombreInstrumentoIsUnique(codigoSbsGenerated, item.IdInstrumentoAdrAds);
                        VerifyInstrumentoAccionAdrAdsIsUnique(item, item.IdAccionAdrAds);
                        var idFecha = Helper.ConvertFechaStringToIdFecha(item.SecuencialFechaAdrAds);

                        var hijo = default(InstrumentoAccion);
                        if (Accion.AccionesHijas.Any(x => x.IndTipoAccionSbs == item.IndTipoAccionSbs))
                        {
                            hijo = Accion.AccionesHijas.FirstOrDefault(x => x.IndTipoAccionSbs == item.IndTipoAccionSbs);
                            if (hijo.Instrumento.IndHabilitacionRiesgo == Habilitado)
                                hijo.Instrumento.IndHabilitacionRiesgo = InabilitadoModificado;
                        }
                        else
                        {
                            hijo = new InstrumentoAccion()
                            {
                                Instrumento = new Instrumento()
                            };

                            Accion.AccionesHijas.Add(hijo);
                            hijo.Instrumento.IndHabilitacionRiesgo = InabilitadoNuevo;
                            hijo.Instrumento.IndActividad = EstadoVigente;
                        }

                        hijo.TieneMonedaDual = Accion.TieneMonedaDual;
                        hijo.IdMonedaDual = Accion.IdMonedaDual == (int)CustomLength.ErrorValue ? new Nullable<int>() : Accion.IdMonedaDual;
                        hijo.IdSecuencialFechaEmision = Accion.IdSecuencialFechaEmision;
                        hijo.IdSecuencialFechaVencimiento = Accion.IdSecuencialFechaVencimiento;
                        hijo.ValorNominal = item.ValorNominal;
                        hijo.ValorNominalSbs = Accion.ValorNominalSbs;
                        hijo.IndTipoCustodia = Accion.IndTipoCustodia;
                        hijo.IndClase = Accion.IndClase;
                        hijo.CodIsin = item.CodIsin;
                        hijo.Nemotecnico = item.Nemotecnico;
                        hijo.MontoColocado = Accion.MontoColocado;
                        hijo.MontoEmitido = item.MontoEmitido;
                        hijo.IdSecuencialFechaMontoEmitido = Accion.IdSecuencialFechaMontoEmitido;
                        hijo.IdSecuencialFechaMontoColocado = Accion.IdSecuencialFechaMontoColocado;
                        hijo.IndTipoUnidadEmision = Accion.IndTipoUnidadEmision;
                        hijo.NroUnidadesFloat = Accion.NroUnidadesFloat;
                        hijo.IndPaisEmisor = Accion.IndPaisEmisor;
                        hijo.IndFocoGeograficoEmision = Accion.IndFocoGeograficoEmision;
                        hijo.IndRegionEmision = Accion.IndRegionEmision;
                        hijo.IndTipoUnidadEmision = Accion.IndTipoUnidadEmision;
                        hijo.TieneFechaVencimiento = Accion.TieneFechaVencimiento;
                        hijo.TieneMandato = Accion.TieneMandato;
                        hijo.IndTipoAccionSbs = item.IndTipoAccionSbs;
                        //hijo.IndNacionalidadAdrAds = item.IndNacionalidadAdrAds;
                        hijo.IdSecuencialFechaAdrAds = idFecha;
                        hijo.FactorConversion = item.FactorConversion;
                        hijo.IsAdrAds = true;
                        hijo.LoginActualizacion = Accion.LoginActualizacion;
                        hijo.FechaHoraActualizacion = DateTime.Now;
                        hijo.UsuarioActualizacion = Constants.UserSystem;

                        hijo.Instrumento.IdTipoInstrumento = instrumentoAccionesDTO.IdTipoInstrumento;
                        hijo.Instrumento.NombreInstrumento = item.Nombre;
                        hijo.Instrumento.CodigoSbs = codigoSbsGenerated;
                        hijo.Instrumento.IdMoneda = item.IdMoneda;
                        hijo.Instrumento.IdEmisor = instrumentoAccionesDTO.IdEmisor;
                        hijo.Instrumento.IdGrupoInstrumento = Instrumento.IdGrupoInstrumento;
                        hijo.Instrumento.IndCategoria = instrumentoAccionesDTO.IndCategoria;
                        hijo.Instrumento.IndFamilia = instrumentoAccionesDTO.IndFamilia;
                        hijo.Instrumento.IdClasificacionRiesgo = instrumentoAccionesDTO.IdClasificacionRiesgo;

                        hijo.Instrumento.LoginActualizacion = instrumentoAccionesDTO.LoginActualizacion;
                        hijo.Instrumento.FechaHoraActualizacion = DateTime.Now;
                        hijo.Instrumento.UsuarioActualizacion = Constants.UserSystem;

                        var nacionalidad = iIndicadorRepository.Get(item.IndTipoAccionSbs);
                        var tipoinstrumento = iTipoInstrumentoRepository.FirstOrDefault(x => x.CodigoSbsTipoInstrumento == nacionalidad.ValorAuxChar1);
                        if (tipoinstrumento != null)
                            hijo.Instrumento.IdTipoInstrumento = tipoinstrumento.IdTipoInstrumento;

                    }
                }
            }
            iInstrumentoAccionRepository.UnitOfWork.Commit();

            /*

            Instrumento persisted = iInstrumentoRepository.Get(instrumentoAccionesDTO.IdInstrumento);
            InstrumentoAccion persistedAccion = iInstrumentoAccionRepository.Get(instrumentoAccionesDTO.IdAccion);

            if (persisted == null || persistedAccion == null)
                throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

            VerifyNombreInstrumentoIsUnique(instrumentoAccionesDTO.CodigoSbs, instrumentoAccionesDTO.IdInstrumento);
            VerifyInstrumentoAccionIsUnique(instrumentoAccionesDTO);

            Indicador indAdr = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.TipoAccionSbs && x.IdIndicador == (int)eTipoAccionSbs.Adr).FirstOrDefault();
            if (indAdr == null)
                throw new Exception(String.Format(mensajeGenericoES.error_SeleccionarElementoYaNoExiste,"Indicador Adr"));	

            Indicador indAds = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.TipoAccionSbs && x.IdIndicador == (int)eTipoAccionSbs.Ads).FirstOrDefault();
            if (indAds == null)
                throw new Exception(String.Format(mensajeGenericoES.error_SeleccionarElementoYaNoExiste,"Indicador Ads"));

            Instrumento persistedPadreAdr = new Instrumento();
            Instrumento persistedPadreAds = new Instrumento();
            InstrumentoAccion persistedAccionHijaAdr = new InstrumentoAccion();
            InstrumentoAccion persistedAccionHijaAds = new InstrumentoAccion();
            if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO != null)
            {
                foreach (var item in instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO)
                {
                    //ADR
                    if (item.IndTipoAccionSbs == indAdr.Id)
                    {
                        if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.FirstOrDefault() != null) 
                            persistedPadreAdr = iInstrumentoRepository.Get(instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.First().IdInstrumentoAdrAds);                            

                        persistedAccionHijaAdr = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == instrumentoAccionesDTO.IdAccion && x.IndTipoAccionSbs == (int)indAdr.Id).FirstOrDefault();                            
                    }
                    //ADS
                    else
                    {
                        if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.LastOrDefault() != null) 
                            persistedPadreAds = iInstrumentoRepository.Get(instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.Last().IdInstrumentoAdrAds);

                        persistedAccionHijaAds = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == instrumentoAccionesDTO.IdAccion && x.IndTipoAccionSbs == (int)indAds.Id).FirstOrDefault();
                    }
                }
            }
            string codigoSbsGenerated = "";
            if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO != null)
            {
                foreach (var item in instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO)
                {
                    codigoSbsGenerated = GeneratedCodigoSbsOnlyFirst7Digits(item.CodigoSbs, item.IdMoneda, instrumentoAccionesDTO.IdEmisor, item.IndNacionalidadAdrAds,1);
                    VerifyNombreInstrumentoIsUnique(codigoSbsGenerated, item.IdInstrumentoAdrAds);
                    VerifyInstrumentoAccionAdrAdsIsUnique(item, item.IdAccionAdrAds);
                }
            }

            int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoAccionesDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
            int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoAccionesDTO.IndHabilitacionRiesgo);

            int IdSecuencialFechaMontoColocado = (instrumentoAccionesDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaMontoColocado);
            int IdSecuencialFechaMontoEmitido = (instrumentoAccionesDTO.SecuencialFechaMontoEmitido.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaMontoEmitido);
            int IdSecuencialFechaEmision = (instrumentoAccionesDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaEmision);
            int? IdSecuencialFechaVencimiento = (instrumentoAccionesDTO.SecuencialFechaVencimiento.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaVencimiento);

            Instrumento current = new Instrumento(instrumentoAccionesDTO.IdTipoInstrumento, instrumentoAccionesDTO.NombreInstrumento, instrumentoAccionesDTO.CodigoSbs,
                instrumentoAccionesDTO.IdMoneda, instrumentoAccionesDTO.IdEmisor, IdGrupoInstrumento, instrumentoAccionesDTO.IndCategoria,
                instrumentoAccionesDTO.IndFamilia, indActividad, instrumentoAccionesDTO.IdClasificacionRiesgo, instrumentoAccionesDTO.LoginActualizacion,
                indHabilitacionRiesgo);
            current.IdInstrumento = persisted.IdInstrumento;
            iInstrumentoRepository.Merge(persisted, current);
            iInstrumentoRepository.UnitOfWork.Commit();
            InstrumentoAccion instrumentoAcciones = new InstrumentoAccion(current.IdInstrumento, instrumentoAccionesDTO.TieneMonedaDual,
                                                                                instrumentoAccionesDTO.IdMonedaDual, IdSecuencialFechaEmision,
                                                                                IdSecuencialFechaVencimiento, instrumentoAccionesDTO.ValorNominal,
                                                                                instrumentoAccionesDTO.ValorNominalSbs, instrumentoAccionesDTO.IndTipoCustodia,
                                                                                instrumentoAccionesDTO.CodIsin, instrumentoAccionesDTO.Nemotecnico,
                                                                                instrumentoAccionesDTO.IndClase, instrumentoAccionesDTO.MontoEmitido, instrumentoAccionesDTO.MontoColocado,
                                                                                IdSecuencialFechaMontoEmitido, IdSecuencialFechaMontoColocado,
                                                                                instrumentoAccionesDTO.IndTipoUnidadEmision, instrumentoAccionesDTO.NroUnidadesFloat,
                                                                                instrumentoAccionesDTO.IndPaisEmisor,
                                                                                instrumentoAccionesDTO.IndFocoGeograficoEmision, instrumentoAccionesDTO.IndRegionEmision,
                                                                                instrumentoAccionesDTO.LoginActualizacion, instrumentoAccionesDTO.TieneFechaVencimiento,
                                                                                instrumentoAccionesDTO.TieneMandato, false);
            instrumentoAcciones.IdAccion = persistedAccion.IdAccion;
            iInstrumentoAccionRepository.Merge(persistedAccion, instrumentoAcciones);
            iInstrumentoAccionRepository.UnitOfWork.Commit();                
            if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO != null)
            {
                foreach (var item in instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO)
                {
                    codigoSbsGenerated = GeneratedCodigoSbsOnlyFirst7Digits(item.CodigoSbs, item.IdMoneda, instrumentoAccionesDTO.IdEmisor, item.IndNacionalidadAdrAds,1);
                    //ADR
                    if (item.IndTipoAccionSbs == indAdr.Id)
                    {
                        Instrumento currentHijaAdr = new Instrumento(instrumentoAccionesDTO.IdTipoInstrumento, item.Nombre,
                            codigoSbsGenerated, item.IdMoneda, instrumentoAccionesDTO.IdEmisor, IdGrupoInstrumento, instrumentoAccionesDTO.IndCategoria,
                            instrumentoAccionesDTO.IndFamilia, indActividad, instrumentoAccionesDTO.IdClasificacionRiesgo, instrumentoAccionesDTO.LoginActualizacion,
                            indHabilitacionRiesgo);

                        if (persistedAccionHijaAdr == null)
                        {//new
                            SaveInstrumento(currentHijaAdr);

                            int idFecha = Helper.ConvertFechaStringToIdFecha(item.SecuencialFechaAdrAds);

                            InstrumentoAccion instrumentoAccionesHijaAdr =
                                new InstrumentoAccion(instrumentoAcciones, currentHijaAdr.IdInstrumento, item.IndTipoAccionSbs, item.Nombre, item.IndNacionalidadAdrAds, item.Nemotecnico,
                                    item.ValorNominal, item.IdMoneda, item.CodIsin, item.MontoEmitido, codigoSbsGenerated, idFecha,
                                    item.FactorConversion, instrumentoAcciones.IdAccion, true);

                            SaveInstrumentoAccion(instrumentoAccionesHijaAdr);
                        }
                        else 
                        {//merge 
                            currentHijaAdr.IdInstrumento = persistedPadreAdr.IdInstrumento;
                            iInstrumentoRepository.Merge(persistedPadreAdr, currentHijaAdr);
                            iInstrumentoRepository.UnitOfWork.Commit();

                            int idFecha = Helper.ConvertFechaStringToIdFecha(item.SecuencialFechaAdrAds);

                            InstrumentoAccion instrumentoAccionesHijaAdr =
                                new InstrumentoAccion(instrumentoAcciones, currentHijaAdr.IdInstrumento, item.IndTipoAccionSbs, item.Nombre, item.IndNacionalidadAdrAds, item.Nemotecnico,
                                    item.ValorNominal, item.IdMoneda, item.CodIsin, item.MontoEmitido, codigoSbsGenerated, idFecha,
                                    item.FactorConversion, instrumentoAcciones.IdAccion, true);

                            instrumentoAccionesHijaAdr.IdAccion = persistedAccionHijaAdr.IdAccion;
                            iInstrumentoAccionRepository.Merge(persistedAccionHijaAdr, instrumentoAccionesHijaAdr);
                            iInstrumentoAccionRepository.UnitOfWork.Commit();
                        }                            
                    }//ADS
                    else
                    {
                        Instrumento currentHijaAds = new Instrumento(instrumentoAccionesDTO.IdTipoInstrumento, item.Nombre,
                            codigoSbsGenerated, item.IdMoneda, instrumentoAccionesDTO.IdEmisor, IdGrupoInstrumento, instrumentoAccionesDTO.IndCategoria,
                            instrumentoAccionesDTO.IndFamilia, indActividad, instrumentoAccionesDTO.IdClasificacionRiesgo, instrumentoAccionesDTO.LoginActualizacion,
                            indHabilitacionRiesgo);

                        if (persistedAccionHijaAds == null)
                        {//new
                            SaveInstrumento(currentHijaAds);

                            int idFecha = Helper.ConvertFechaStringToIdFecha(item.SecuencialFechaAdrAds);

                            InstrumentoAccion instrumentoAccionesHijaAds =
                                new InstrumentoAccion(instrumentoAcciones, currentHijaAds.IdInstrumento, item.IndTipoAccionSbs, item.Nombre, item.IndNacionalidadAdrAds, item.Nemotecnico,
                                    item.ValorNominal, item.IdMoneda, item.CodIsin, item.MontoEmitido, codigoSbsGenerated, idFecha,
                                    item.FactorConversion, instrumentoAcciones.IdAccion, true);

                            SaveInstrumentoAccion(instrumentoAccionesHijaAds);
                        }
                        else
                        {//merge 
                            currentHijaAds.IdInstrumento = persistedPadreAds.IdInstrumento;
                            iInstrumentoRepository.Merge(persistedPadreAds, currentHijaAds);
                            iInstrumentoRepository.UnitOfWork.Commit();

                            int idFecha = Helper.ConvertFechaStringToIdFecha(item.SecuencialFechaAdrAds);

                            InstrumentoAccion instrumentoAccionesHijaAds =
                                new InstrumentoAccion(instrumentoAcciones, currentHijaAds.IdInstrumento, item.IndTipoAccionSbs, item.Nombre, item.IndNacionalidadAdrAds, item.Nemotecnico,
                                    item.ValorNominal, item.IdMoneda, item.CodIsin, item.MontoEmitido, codigoSbsGenerated, idFecha,
                                    item.FactorConversion, instrumentoAcciones.IdAccion, true);

                            instrumentoAccionesHijaAds.IdAccion = persistedAccionHijaAds.IdAccion;
                            iInstrumentoAccionRepository.Merge(persistedAccionHijaAds, instrumentoAccionesHijaAds);
                            iInstrumentoAccionRepository.UnitOfWork.Commit();
                        }
                    }
                }
            }
            transactionScope.Complete();
        }*/
            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public InstrumentoAccionDTO[] GetAllAccionByActiveRiesgo(string grupo)
        {
            GrupoInstrumentoDTO grupoInstrumentoDTO = iGrupoInstrumentoAppService.GetByGrupo(grupo);
            if (grupoInstrumentoDTO == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);
            IEnumerable<Instrumento> instrumentos = iInstrumentoRepository.GetFiltered(i => i.IndActividad.Equals(indActividad) &&
                                                                                            i.IndHabilitacionRiesgo == indHabilitacionRiesgo &&
                                                                                            i.IdGrupoInstrumento.Value == grupoInstrumentoDTO.IdGrupoInstrumento)
                                                                          .ToList();
            if (instrumentos == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            foreach (Instrumento instrumento in instrumentos)
            {
                instrumento.Accion = iInstrumentoAccionRepository.GetFiltered(obj => obj.IdInstrumento.Equals(instrumento.IdInstrumento)).FirstOrDefault();
            }

            List<InstrumentoAccionDTO> accionesDTO = instrumentos.ProjectedAsCollection<InstrumentoAccionDTO>().ToList();

            //InstrumentoAccionAdrAdsDTO accionHija;
            //List<InstrumentoAccionAdrAdsDTO> accionesHijas = new List<InstrumentoAccionAdrAdsDTO>();
            //if (accionesDTO != null)
            //{
            //    foreach (var item in accionesDTO)
            //    {                    
            //        Instrumento instrumentoHija = iInstrumentoRepository.Get(item.IdInstrumento);
            //        string fecha = Helper.ConvertIdFechaToFechaString(item.IdSecuencialFechaAdrAds);
            //        accionHija = new InstrumentoAccionAdrAdsDTO();
            //        accionHija.IdAccionAdrAds = item.IdAccion;
            //        accionHija.IdInstrumentoAdrAds = item.IdInstrumento;
            //        accionHija.IndTipoAccionSbs = item.IndTipoAccionSbs == null ? 0 : (int)item.IndTipoAccionSbs;
            //        accionHija.Nombre = instrumentoHija.NombreInstrumento;
            //        accionHija.IndNacionalidadAdrAds = item.IndNacionalidadAdrAds == null ? 0 : (int)item.IndNacionalidadAdrAds;
            //        accionHija.Nemotecnico = item.Nemotecnico;
            //        accionHija.ValorNominal = item.ValorNominal;
            //        accionHija.IdMoneda = instrumentoHija.IdMoneda;
            //        accionHija.CodIsin = item.CodIsin;
            //        accionHija.MontoEmitido = item.MontoEmitido;
            //        accionHija.CodigoSbs = instrumentoHija.CodigoSbs;
            //        accionHija.SecuencialFechaAdrAds = fecha;
            //        accionHija.IdSecuencialFechaAdrAds = item.IdSecuencialFechaAdrAds == null ? 0 : (int)item.IdSecuencialFechaAdrAds;
            //        accionHija.FactorConversion = item.FactorConversion == null ? 0 : (decimal)item.FactorConversion;                    
            //        accionesHijas.Add(accionHija);
            //    }                
            //    accionesDTO.ListadoInstrumentoAccionAdsAdsDTO = accionesHijas;
            //}


            return accionesDTO.ToArray();
        }
        public InstrumentoAccionDTO GetByIdInstrumentoAccion(int idInstrumento, int idAccion)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.Accion = iInstrumentoAccionRepository.Get(idAccion);

            if (instrumento.Accion == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string SecuencialFechaEmision = Helper.ConvertIdFechaToFechaString(instrumento.Accion.IdSecuencialFechaEmision);
            string SecuencialFechaVencimiento = instrumento.Accion.IdSecuencialFechaVencimiento == null ? string.Empty : Helper.ConvertIdFechaToFechaString(instrumento.Accion.IdSecuencialFechaVencimiento.Value);
            string SecuencialFechaMontoEmitido = Helper.ConvertIdFechaToFechaString(instrumento.Accion.IdSecuencialFechaMontoEmitido);
            string SecuencialFechaMontoColocado = Helper.ConvertIdFechaToFechaString(instrumento.Accion.IdSecuencialFechaMontoColocado);

            instrumento.Accion.SecuencialFechaEmision = SecuencialFechaEmision;
            instrumento.Accion.SecuencialFechaVencimiento = SecuencialFechaVencimiento;
            instrumento.Accion.SecuencialFechaMontoEmitido = SecuencialFechaMontoEmitido;
            instrumento.Accion.SecuencialFechaMontoColocado = SecuencialFechaMontoColocado;

            InstrumentoAccionDTO accionDTO = instrumento.ProjectedAs<InstrumentoAccionDTO>();

            if (instrumento.Accion.IsAdrAds != null && instrumento.Accion.IsAdrAds.Value)
            {
                InstrumentoAccionAdrAdsDTO accionHija;
                var accionesHijas = new List<InstrumentoAccionAdrAdsDTO>();
                var fecha = Helper.ConvertIdFechaToFechaString(instrumento.Accion.IdSecuencialFechaAdrAds);
                accionHija = new InstrumentoAccionAdrAdsDTO
                {
                    IdAccionAdrAds = instrumento.Accion.IdAccion,
                    IdInstrumentoAdrAds = instrumento.Accion.IdInstrumento,
                    IndTipoAccionSbs = instrumento.Accion.IndTipoAccionSbs == null ? 0 : (int)instrumento.Accion.IndTipoAccionSbs,
                    Nombre = instrumento.NombreInstrumento,
                    IdTipoInstrumento = instrumento.IdTipoInstrumento.Value,
                    Nemotecnico = instrumento.Accion.Nemotecnico,
                    ValorNominal = instrumento.Accion.ValorNominal,
                    IdMoneda = instrumento.IdMoneda,
                    CodIsin = instrumento.Accion.CodIsin,
                    MontoEmitido = instrumento.Accion.MontoEmitido,
                    CodigoSbs = instrumento.CodigoSbs,
                    SecuencialFechaAdrAds = fecha,
                    IdSecuencialFechaAdrAds = instrumento.Accion.IdSecuencialFechaAdrAds == null ? 0 : (int)instrumento.Accion.IdSecuencialFechaAdrAds,
                    FactorConversion = instrumento.Accion.FactorConversion == null ? 0 : (decimal)instrumento.Accion.FactorConversion,
                    IndHabilitacionRiesgo = instrumento.IndHabilitacionRiesgo,
                    ComentarioHabilitacion = instrumento.ComentarioHabilitacion
                };
                accionesHijas.Add(accionHija);
                accionDTO.ListadoInstrumentoAccionAdsAdsDTO = accionesHijas;
            }
            else
            {
                List<InstrumentoAccion> acciones = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == idAccion).ToList();
                InstrumentoAccionAdrAdsDTO accionHija;
                List<InstrumentoAccionAdrAdsDTO> accionesHijas = new List<InstrumentoAccionAdrAdsDTO>();
                if (acciones != null)
                {
                    foreach (var item in acciones)
                    {
                        Instrumento instrumentoHija = iInstrumentoRepository.Get(item.IdInstrumento);
                        string fecha = Helper.ConvertIdFechaToFechaString(item.IdSecuencialFechaAdrAds);
                        accionHija = new InstrumentoAccionAdrAdsDTO();
                        accionHija.IdAccionAdrAds = item.IdAccion;
                        accionHija.IdInstrumentoAdrAds = item.IdInstrumento;
                        accionHija.IndTipoAccionSbs = item.IndTipoAccionSbs == null ? 0 : (int)item.IndTipoAccionSbs;
                        accionHija.Nombre = instrumentoHija.NombreInstrumento;
                        accionHija.IdTipoInstrumento = instrumentoHija.IdTipoInstrumento.Value;
                        accionHija.Nemotecnico = item.Nemotecnico;
                        accionHija.ValorNominal = item.ValorNominal;
                        accionHija.IdMoneda = instrumentoHija.IdMoneda;
                        accionHija.CodIsin = item.CodIsin;
                        accionHija.MontoEmitido = item.MontoEmitido;
                        accionHija.CodigoSbs = instrumentoHija.CodigoSbs;
                        accionHija.SecuencialFechaAdrAds = fecha;
                        accionHija.IdSecuencialFechaAdrAds = item.IdSecuencialFechaAdrAds == null ? 0 : (int)item.IdSecuencialFechaAdrAds;
                        accionHija.FactorConversion = item.FactorConversion == null ? 0 : (decimal)item.FactorConversion;
                        accionHija.IndHabilitacionRiesgo = instrumentoHija.IndHabilitacionRiesgo;
                        accionHija.ComentarioHabilitacion = instrumentoHija.ComentarioHabilitacion;
                        accionesHijas.Add(accionHija);
                    }
                    accionDTO.ListadoInstrumentoAccionAdsAdsDTO = accionesHijas;
                }
            }
            return accionDTO;
        }
        public string AddNewInstrumentoAccion(InstrumentoAccionDTO instrumentoAccionesDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoAccionesDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);

                VerifyNombreInstrumentoIsUnique(instrumentoAccionesDTO.CodigoSbs, instrumentoAccionesDTO.IdInstrumento);
                VerifyInstrumentoAccionIsUnique(instrumentoAccionesDTO);

                string codigoSbsGenerated = "";
                if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO != null)
                {
                    foreach (var item in instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO)
                    {
                        codigoSbsGenerated = GeneratedCodigoSbsOnlyFirst7Digits(item.CodigoSbs, item.IdMoneda, instrumentoAccionesDTO.IdEmisor, item.IdTipoInstrumento, 1);

                        VerifyNombreInstrumentoIsUnique(codigoSbsGenerated, instrumentoAccionesDTO.IdInstrumento);
                        VerifyInstrumentoAccionAdrAdsIsUnique(item, instrumentoAccionesDTO.IdAccion);
                    }
                }
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoAccionesDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;

                int IdSecuencialFechaMontoColocado = (instrumentoAccionesDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaMontoColocado);
                int IdSecuencialFechaMontoEmitido = (instrumentoAccionesDTO.SecuencialFechaMontoEmitido.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaMontoEmitido);
                int IdSecuencialFechaEmision = (instrumentoAccionesDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaEmision);
                int? IdSecuencialFechaVencimiento = (instrumentoAccionesDTO.SecuencialFechaVencimiento.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoAccionesDTO.SecuencialFechaVencimiento);


                Instrumento instrumento = new Instrumento(instrumentoAccionesDTO.IdTipoInstrumento, instrumentoAccionesDTO.NombreInstrumento, instrumentoAccionesDTO.CodigoSbs,
                    instrumentoAccionesDTO.IdMoneda, instrumentoAccionesDTO.IdEmisor, IdGrupoInstrumento, instrumentoAccionesDTO.IndCategoria,
                    instrumentoAccionesDTO.IndFamilia, indActividad, instrumentoAccionesDTO.IdClasificacionRiesgo, instrumentoAccionesDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                SaveInstrumento(instrumento);

                InstrumentoAccion instrumentoAcciones = new InstrumentoAccion(instrumento.IdInstrumento, instrumentoAccionesDTO.TieneMonedaDual,
                                                                                    instrumentoAccionesDTO.IdMonedaDual, IdSecuencialFechaEmision,
                                                                                    IdSecuencialFechaVencimiento, instrumentoAccionesDTO.ValorNominal,
                                                                                    instrumentoAccionesDTO.ValorNominalSbs, instrumentoAccionesDTO.IndTipoCustodia,
                                                                                    instrumentoAccionesDTO.CodIsin, instrumentoAccionesDTO.Nemotecnico,
                                                                                    instrumentoAccionesDTO.IndClase, instrumentoAccionesDTO.MontoEmitido, instrumentoAccionesDTO.MontoColocado,
                                                                                    IdSecuencialFechaMontoEmitido, IdSecuencialFechaMontoColocado,
                                                                                    instrumentoAccionesDTO.IndTipoUnidadEmision, instrumentoAccionesDTO.NroUnidadesFloat,
                                                                                    instrumentoAccionesDTO.IndPaisEmisor,
                                                                                    instrumentoAccionesDTO.IndFocoGeograficoEmision, instrumentoAccionesDTO.IndRegionEmision,
                                                                                    instrumentoAccionesDTO.LoginActualizacion, instrumentoAccionesDTO.TieneFechaVencimiento,
                                                                                    instrumentoAccionesDTO.TieneMandato, false);

                SaveInstrumentoAccion(instrumentoAcciones);

                if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO != null)
                {
                    foreach (var item in instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO)
                    {
                        codigoSbsGenerated = GeneratedCodigoSbsOnlyFirst7Digits(item.CodigoSbs, item.IdMoneda, instrumentoAccionesDTO.IdEmisor, item.IdTipoInstrumento, 1);

                        Instrumento instrumentoHija = new Instrumento(instrumentoAccionesDTO.IdTipoInstrumento, item.Nombre,
                            codigoSbsGenerated, item.IdMoneda, instrumentoAccionesDTO.IdEmisor, IdGrupoInstrumento,
                            instrumentoAccionesDTO.IndCategoria, instrumentoAccionesDTO.IndFamilia, indActividad, instrumentoAccionesDTO.IdClasificacionRiesgo,
                            instrumentoAccionesDTO.LoginActualizacion, indHabilitacionRiesgo);

                        SaveInstrumento(instrumentoHija);

                        int idFecha = Helper.ConvertFechaStringToIdFecha(item.SecuencialFechaAdrAds);

                        InstrumentoAccion instrumentoAccionesHija =
                            new InstrumentoAccion(instrumentoAcciones, instrumentoHija.IdInstrumento, item.IndTipoAccionSbs, item.Nombre, item.IdTipoInstrumento, item.Nemotecnico,
                                item.ValorNominal, item.IdMoneda, item.CodIsin, item.MontoEmitido, codigoSbsGenerated, idFecha,
                                item.FactorConversion, instrumentoAcciones.IdAccion, true);

                        /*      var nacionalidad = iIndicadorRepository.Get(item.IndTipoAccionSbs);
                              var tipoinstrumento = iTipoInstrumentoRepository.FirstOrDefault(x => x.CodigoSbsTipoInstrumento == nacionalidad.ValorAuxChar1);
                              if (tipoinstrumento != null)*/
                        instrumentoHija.IdTipoInstrumento = item.IdTipoInstrumento;

                        SaveInstrumentoAccion(instrumentoAccionesHija);


                    }
                }
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_RegistrarElemento;
        }
        public string RemoveInstrumentoAccion(int idInstrumento, int idAccion)
        {
            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);
            InstrumentoAccion persistedAccion = iInstrumentoAccionRepository.Get(idAccion);

            if (persisted == null || persistedAccion == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_Accion_Tiene_Dependencias));

            Indicador indAdr = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.TipoAccionSbs && x.IdIndicador == (int)eTipoAccionSbs.Adr).FirstOrDefault();
            if (indAdr == null)
                throw new Exception(String.Format(mensajeGenericoES.error_SeleccionarElementoYaNoExiste, "Indicador Adr"));

            Indicador indAds = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.TipoAccionSbs && x.IdIndicador == (int)eTipoAccionSbs.Ads).FirstOrDefault();
            if (indAds == null)
                throw new Exception(String.Format(mensajeGenericoES.error_SeleccionarElementoYaNoExiste, "Indicador Ads"));

            List<InstrumentoAccion> persistedAccionAdrAds = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == idAccion).ToList();

            Instrumento persistedPadreAdr = new Instrumento();
            Instrumento persistedPadreAds = new Instrumento();
            InstrumentoAccion persistedAccionHijaAdr = new InstrumentoAccion();
            InstrumentoAccion persistedAccionHijaAds = new InstrumentoAccion();
            if (persistedAccionAdrAds != null)
            {
                foreach (var item in persistedAccionAdrAds)
                {
                    //ADR
                    if (item.IndTipoAccionSbs == indAdr.Id)
                    {
                        persistedPadreAdr = iInstrumentoRepository.Get(item.IdInstrumento);
                        persistedAccionHijaAdr = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccion == item.IdAccion && x.IndTipoAccionSbs == (int)indAdr.Id).FirstOrDefault();

                        if (persistedAccionHijaAdr != null)
                            iInstrumentoAccionRepository.Remove(persistedAccionHijaAdr);
                        if (persistedPadreAdr != null)
                            iInstrumentoRepository.RemoveInstrumentoOnCascade(persistedPadreAdr);

                        iInstrumentoRepository.UnitOfWork.Commit();
                    }
                    //ADS
                    else
                    {
                        persistedPadreAds = iInstrumentoRepository.Get(item.IdInstrumento);
                        persistedAccionHijaAds = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccion == item.IdAccion && x.IndTipoAccionSbs == (int)indAds.Id).FirstOrDefault();

                        if (persistedAccionHijaAds != null)
                            iInstrumentoAccionRepository.Remove(persistedAccionHijaAds);
                        if (persistedPadreAds != null)
                            iInstrumentoRepository.RemoveInstrumentoOnCascade(persistedPadreAds);

                        iInstrumentoRepository.UnitOfWork.Commit();
                    }
                }
            }

            iInstrumentoAccionRepository.Remove(persistedAccion);
            iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
            iInstrumentoRepository.UnitOfWork.Commit();
            return mensajeGenericoES.exito_EliminarElemento;
        }
        public string AnnulInstrumentoAccion(InstrumentoAccionDTO instrumentoAccionesDTO)
        {
            /* using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
             {*/
            if (instrumentoAccionesDTO.IdInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            var idAnulado = iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
            var indHabilitacionRiesgo = iIndicadorRepository.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

            var instrumento = iInstrumentoRepository.GetInstrumentoAccion(instrumentoAccionesDTO.IdInstrumento);

            instrumento.IndActividad = idAnulado;
            instrumento.ComentarioAnulacion = instrumentoAccionesDTO.ComentarioAnulacion;
            instrumento.LoginActualizacion = instrumentoAccionesDTO.LoginActualizacion;
            instrumento.FechaHoraActualizacion = DateTime.Now;
            instrumento.UsuarioActualizacion = Constants.UserSystem;

            var Accion = iInstrumentoAccionRepository.Get(instrumentoAccionesDTO.IdAccion);

            if (iInstrumentoAccionRepository.Any(x => x.IdAccionPrincipal == Accion.IdAccion))
            {
                //var comment = "";
                foreach (var hijo in iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == Accion.IdAccion).ToArray())
                {
                    hijo.Instrumento.IndActividad = idAnulado;
                    hijo.Instrumento.ComentarioAnulacion = instrumentoAccionesDTO.ComentarioAnulacion;
                    hijo.Instrumento.LoginActualizacion = instrumentoAccionesDTO.LoginActualizacion;
                    hijo.Instrumento.FechaHoraActualizacion = DateTime.Now;
                    hijo.Instrumento.UsuarioActualizacion = Constants.UserSystem;
                }
            }
            iInstrumentoAccionRepository.UnitOfWork.Commit();
            /* 
             InstrumentoAccion persistedAccion = iInstrumentoAccionRepository.Get(instrumentoAccionesDTO.IdAccion);

             if (persisted == null || persistedAccion == null)
                 throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);
            */
            /*

                            Indicador indAdr = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.TipoAccionSbs && x.IdIndicador == (int)eTipoAccionSbs.Adr).FirstOrDefault();
                            if (indAdr == null)
                                throw new Exception(String.Format(mensajeGenericoES.error_SeleccionarElementoYaNoExiste, "Indicador Adr"));

                            Indicador indAds = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.TipoAccionSbs && x.IdIndicador == (int)eTipoAccionSbs.Ads).FirstOrDefault();
                            if (indAds == null)
                                throw new Exception(String.Format(mensajeGenericoES.error_SeleccionarElementoYaNoExiste, "Indicador Ads"));

                            Instrumento persistedPadreAdr = new Instrumento();
                            Instrumento persistedPadreAds = new Instrumento();
                            InstrumentoAccion persistedAccionHijaAdr = new InstrumentoAccion();
                            InstrumentoAccion persistedAccionHijaAds = new InstrumentoAccion();
                            string comment = "";
                            if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO != null)
                            {
                                foreach (var item in instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO)
                                {
                                    //ADR
                                    if (item.IndTipoAccionSbs == indAdr.Id)
                                    {
                                        if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.FirstOrDefault() != null) 
                                            persistedPadreAdr = iInstrumentoRepository.Get(instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.First().IdInstrumentoAdrAds);

                                        persistedAccionHijaAdr = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == instrumentoAccionesDTO.IdAccion && x.IndTipoAccionSbs == (int)indAdr.Id).FirstOrDefault();

                                        comment = string.IsNullOrEmpty(item.ComentarioAnulacion) ? instrumentoAccionesDTO.ComentarioAnulacion : item.ComentarioAnulacion;

                                        Instrumento currentHijaAdr = new Instrumento(comment, instrumentoAccionesDTO.LoginActualizacion);
                                        if (persistedPadreAdr != null)
                                        {
                                            currentHijaAdr.IdInstrumento = persistedPadreAdr.IdInstrumento;

                                            persistedPadreAdr.IndActividad = idAnulado;
                                            persistedPadreAdr.ComentarioAnulacion = currentHijaAdr.ComentarioAnulacion;
                                            persistedPadreAdr.LoginActualizacion = currentHijaAdr.LoginActualizacion;
                                            persistedPadreAdr.FechaHoraActualizacion = currentHijaAdr.FechaHoraActualizacion;
                                            persistedPadreAdr.UsuarioActualizacion = currentHijaAdr.UsuarioActualizacion;

                                            persistedAccionHijaAdr.LoginActualizacion = currentHijaAdr.LoginActualizacion;
                                            persistedAccionHijaAdr.FechaHoraActualizacion = currentHijaAdr.FechaHoraActualizacion;
                                            persistedAccionHijaAdr.UsuarioActualizacion = currentHijaAdr.UsuarioActualizacion;

                                            iInstrumentoRepository.Merge(persistedPadreAdr, persistedPadreAdr);
                                            iInstrumentoRepository.UnitOfWork.Commit();
                                            iInstrumentoAccionRepository.Merge(persistedAccionHijaAdr, persistedAccionHijaAdr);
                                            iInstrumentoAccionRepository.UnitOfWork.Commit();
                                        }
                                    }
                                    //ADS
                                    else
                                    {
                                        if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.LastOrDefault() != null) 
                                            persistedPadreAds = iInstrumentoRepository.Get(instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.Last().IdInstrumentoAdrAds);

                                        persistedAccionHijaAds = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == instrumentoAccionesDTO.IdAccion && x.IndTipoAccionSbs == (int)indAds.Id).FirstOrDefault();

                                        comment = String.IsNullOrEmpty(item.ComentarioAnulacion) ? instrumentoAccionesDTO.ComentarioAnulacion : item.ComentarioAnulacion;

                                        Instrumento currentHijaAds = new Instrumento(comment, instrumentoAccionesDTO.LoginActualizacion);
                                        if (persistedPadreAds != null)
                                        {
                                            currentHijaAds.IdInstrumento = persistedPadreAds.IdInstrumento;

                                            persistedPadreAds.IndActividad = idAnulado;
                                            persistedPadreAds.ComentarioAnulacion = currentHijaAds.ComentarioAnulacion;
                                            persistedPadreAds.LoginActualizacion = currentHijaAds.LoginActualizacion;
                                            persistedPadreAds.FechaHoraActualizacion = currentHijaAds.FechaHoraActualizacion;
                                            persistedPadreAds.UsuarioActualizacion = currentHijaAds.UsuarioActualizacion;

                                            persistedAccionHijaAds.LoginActualizacion = currentHijaAds.LoginActualizacion;
                                            persistedAccionHijaAds.FechaHoraActualizacion = currentHijaAds.FechaHoraActualizacion;
                                            persistedAccionHijaAds.UsuarioActualizacion = currentHijaAds.UsuarioActualizacion;

                                            iInstrumentoRepository.Merge(persistedPadreAds, persistedPadreAds);
                                            iInstrumentoRepository.UnitOfWork.Commit();
                                            iInstrumentoAccionRepository.Merge(persistedAccionHijaAds, persistedAccionHijaAds);
                                            iInstrumentoAccionRepository.UnitOfWork.Commit();
                                        }
                                    }
                                }
                            }

                            Instrumento current = new Instrumento(instrumentoAccionesDTO.ComentarioAnulacion,
                                instrumentoAccionesDTO.LoginActualizacion);
                            current.IdInstrumento = persisted.IdInstrumento;

                            persisted.IndActividad = idAnulado;
                            persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                            persisted.LoginActualizacion = current.LoginActualizacion;
                            persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                            persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                            persistedAccion.LoginActualizacion = current.LoginActualizacion;
                            persistedAccion.FechaHoraActualizacion = current.FechaHoraActualizacion;
                            persistedAccion.UsuarioActualizacion = current.UsuarioActualizacion;

                            iInstrumentoRepository.Merge(persisted, persisted);
                            iInstrumentoRepository.UnitOfWork.Commit();
                            iInstrumentoAccionRepository.Merge(persistedAccion, persistedAccion);*/
            /*
            transactionScope.Complete();
        }*/
            return mensajeGenericoES.exito_AnularElemento;
        }
        public string ActiveInstrumentoAccion(InstrumentoAccionDTO instrumentoAccionesDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoAccionesDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoAccionesDTO.IdInstrumento);
                if (!iAnexoIRubroRepository.Any(p => p.AnexoIRubroDetalleInstrumento.Any(q => q.IdTipoInstrumento == persisted.TipoInstrumento.IdTipoInstrumento && q.IdEmisor == persisted.IdEmisor)))
                    throw new ApplicationException("No tiene detalle en el Anexo III");
                InstrumentoAccion persistedAccion = iInstrumentoAccionRepository.Get(instrumentoAccionesDTO.IdAccion);

                if (persisted == null || persistedAccion == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoAccionesDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoAccionesDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;


                //Segun analistas funcionales se indica que la habilitacion seria independiente para hijos y padres

                ////Indicador indAdr = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.TipoAccionSbs && x.IdIndicador == (int)eTipoAccionSbs.Adr).FirstOrDefault();
                ////if (indAdr == null)
                ////    throw new Exception(String.Format(mensajeGenericoES.error_SeleccionarElementoYaNoExiste, "Indicador Adr"));

                ////Indicador indAds = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.TipoAccionSbs && x.IdIndicador == (int)eTipoAccionSbs.Ads).FirstOrDefault();
                ////if (indAds == null)
                ////    throw new Exception(String.Format(mensajeGenericoES.error_SeleccionarElementoYaNoExiste, "Indicador Ads"));

                ////Instrumento persistedPadreAdr = new Instrumento();
                ////Instrumento persistedPadreAds = new Instrumento();
                ////InstrumentoAccion persistedAccionHijaAdr = new InstrumentoAccion();
                ////InstrumentoAccion persistedAccionHijaAds = new InstrumentoAccion();
                ////if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO != null)
                ////{
                ////    foreach (var item in instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO)
                ////    {
                ////        //ADR
                ////        if (item.IndTipoAccionSbs == indAdr.Id)
                ////        {
                ////            if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.FirstOrDefault() != null)                            
                ////                persistedPadreAdr = iInstrumentoRepository.Get(instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.First().IdInstrumentoAdrAds);    

                ////            persistedAccionHijaAdr = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == instrumentoAccionesDTO.IdAccion && x.IndTipoAccionSbs == (int)indAdr.Id).FirstOrDefault();

                ////            Instrumento currentHijaAdr = new Instrumento(instrumentoAccionesDTO.ComentarioHabilitacion, instrumentoAccionesDTO.LoginActualizacion, indHabilitacionRiesgo);
                ////            if (persistedPadreAdr != null)
                ////            {
                ////                currentHijaAdr.IdInstrumento = persistedPadreAdr.IdInstrumento;

                ////                persistedPadreAdr.IndHabilitacionRiesgo = currentHijaAdr.IndHabilitacionRiesgo;
                ////                persistedPadreAdr.ComentarioHabilitacion = currentHijaAdr.ComentarioHabilitacion;
                ////                persistedPadreAdr.LoginActualizacion = currentHijaAdr.LoginActualizacion;
                ////                persistedPadreAdr.FechaHoraActualizacion = currentHijaAdr.FechaHoraActualizacion;
                ////                persistedPadreAdr.UsuarioActualizacion = currentHijaAdr.UsuarioActualizacion;

                ////                persistedAccionHijaAdr.LoginActualizacion = currentHijaAdr.LoginActualizacion;
                ////                persistedAccionHijaAdr.FechaHoraActualizacion = currentHijaAdr.FechaHoraActualizacion;
                ////                persistedAccionHijaAdr.UsuarioActualizacion = currentHijaAdr.UsuarioActualizacion;

                ////                iInstrumentoRepository.Merge(persistedPadreAdr, persistedPadreAdr);
                ////                iInstrumentoRepository.UnitOfWork.Commit();
                ////                iInstrumentoAccionRepository.Merge(persistedAccionHijaAdr, persistedAccionHijaAdr);
                ////                iInstrumentoAccionRepository.UnitOfWork.Commit();
                ////            }
                ////        }
                ////        //ADS
                ////        else
                ////        {
                ////            if (instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.LastOrDefault() != null) 
                ////                persistedPadreAds = iInstrumentoRepository.Get(instrumentoAccionesDTO.ListadoInstrumentoAccionAdsAdsDTO.Last().IdInstrumentoAdrAds);

                ////            persistedAccionHijaAds = iInstrumentoAccionRepository.GetFiltered(x => x.IdAccionPrincipal == instrumentoAccionesDTO.IdAccion && x.IndTipoAccionSbs == (int)indAds.Id).FirstOrDefault();

                ////            Instrumento currentHijaAds = new Instrumento(instrumentoAccionesDTO.ComentarioHabilitacion, instrumentoAccionesDTO.LoginActualizacion, indHabilitacionRiesgo);
                ////            if (persistedPadreAds != null)
                ////            {
                ////                currentHijaAds.IdInstrumento = persistedPadreAds.IdInstrumento;

                ////                persistedPadreAds.IndHabilitacionRiesgo = currentHijaAds.IndHabilitacionRiesgo;
                ////                persistedPadreAds.ComentarioHabilitacion = currentHijaAds.ComentarioHabilitacion;
                ////                persistedPadreAds.LoginActualizacion = currentHijaAds.LoginActualizacion;
                ////                persistedPadreAds.FechaHoraActualizacion = currentHijaAds.FechaHoraActualizacion;
                ////                persistedPadreAds.UsuarioActualizacion = currentHijaAds.UsuarioActualizacion;

                ////                persistedAccionHijaAds.LoginActualizacion = currentHijaAds.LoginActualizacion;
                ////                persistedAccionHijaAds.FechaHoraActualizacion = currentHijaAds.FechaHoraActualizacion;
                ////                persistedAccionHijaAds.UsuarioActualizacion = currentHijaAds.UsuarioActualizacion;

                ////                iInstrumentoRepository.Merge(persistedPadreAds, persistedPadreAds);
                ////                iInstrumentoRepository.UnitOfWork.Commit();
                ////                iInstrumentoAccionRepository.Merge(persistedAccionHijaAds, persistedAccionHijaAds);
                ////                iInstrumentoAccionRepository.UnitOfWork.Commit();
                ////            }
                ////        }
                ////    }
                ////}

                Instrumento current = new Instrumento(instrumentoAccionesDTO.ComentarioHabilitacion,
                    instrumentoAccionesDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedAccion.LoginActualizacion = current.LoginActualizacion;
                persistedAccion.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedAccion.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoAccionRepository.Merge(persistedAccion, persistedAccion);
                iInstrumentoAccionRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensaje;
        }

        #endregion

        #region InstrumentoCertificadoSuscripcionPreferenteAppService Members

        public InstrumentoCertificadoSuscripcionPreferentePagedDTO GetFilteredDataCertificadoSP(string codigoSbs, int idEmisor, int idValorAsociado, string fechaCorte,
            string fechaIniNeg, string fechaFinNeg, int indActividad, int indHabilitacion, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;

            int idFechaCorte = (fechaCorte.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fechaCorte);
            int idFechaIniNeg = (fechaIniNeg.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fechaIniNeg);
            int idFechaFinNeg = (fechaFinNeg.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fechaFinNeg);
            InstrumentoCertificadoSuscripcionPreferentePagedDTO response =
                iInstrumentoDataRepository.GetFilteredDataCertificadoSP(codigoSbs, idEmisor, idValorAsociado, idFechaCorte, idFechaIniNeg, idFechaFinNeg, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);

            foreach (InstrumentoCertificadoSuscripcionPreferenteListadoDTO item in response.ListaInstrumentoCertificadoSuscripcionPreferente)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }

            return response;
        }
        public string UpdateCertificadoSP(InstrumentoCertificadoSuscripcionPreferenteDTO instrumentoCertificadoSpDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoCertificadoSpDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoCertificadoSpDTO"));

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoCertificadoSpDTO.IdInstrumento);
                InstrumentoCertificadoSuscripcionPreferente persistedCertificadoSuscripcionPreferente = iInstrumentoCertificadoSuscripcionPreferenteRepository.Get(instrumentoCertificadoSpDTO.IdCertificadoSuscripcionPreferente);
                if (persisted == null || persistedCertificadoSuscripcionPreferente == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                VerifyNombreInstrumentoIsUnique(instrumentoCertificadoSpDTO.CodigoSbs, instrumentoCertificadoSpDTO.IdInstrumento);
                VerifyInstrumentoCertificadoSuscripcionPreferenteIsUnique(instrumentoCertificadoSpDTO);

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoCertificadoSpDTO.IndHabilitacionRiesgo);

                int IdSecuencialFechaAcuerdo = (instrumentoCertificadoSpDTO.SecuencialFechaAcuerdo.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaAcuerdo);
                int IdSecuencialFechaInicioNegociacion = (instrumentoCertificadoSpDTO.SecuencialFechaInicioNegociacion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaInicioNegociacion);
                int IdSecuencialFechaCorte = (instrumentoCertificadoSpDTO.SecuencialFechaCorte.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaCorte);
                int IdSecuencialFechaFinNegociacion = (instrumentoCertificadoSpDTO.SecuencialFechaFinNegociacion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaFinNegociacion);
                int IdSecuencialFechaRegistro = (instrumentoCertificadoSpDTO.SecuencialFechaRegistro.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaRegistro);
                int IdSecuencialFechaMontoEmitido = (instrumentoCertificadoSpDTO.SecuencialFechaMontoEmitido.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaMontoEmitido);
                int IdSecuencialFechaEntrega = (instrumentoCertificadoSpDTO.SecuencialFechaEntrega.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaEntrega);
                int IdSecuencialFechaMontoColocado = (instrumentoCertificadoSpDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaMontoColocado);
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoCertificadoSpDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                Instrumento current = new Instrumento(instrumentoCertificadoSpDTO.IdTipoInstrumento, instrumentoCertificadoSpDTO.NombreInstrumento,
                    instrumentoCertificadoSpDTO.CodigoSbs, instrumentoCertificadoSpDTO.IdMoneda, instrumentoCertificadoSpDTO.IdEmisor,
                    IdGrupoInstrumento, instrumentoCertificadoSpDTO.IndCategoria, instrumentoCertificadoSpDTO.IndFamilia,
                    indActividad, instrumentoCertificadoSpDTO.IdClasificacionRiesgo, instrumentoCertificadoSpDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, current);
                iInstrumentoRepository.UnitOfWork.Commit();

                InstrumentoCertificadoSuscripcionPreferente instrumentoCertificadoSp = new InstrumentoCertificadoSuscripcionPreferente(
                    current.IdInstrumento, instrumentoCertificadoSpDTO.IdAccion,
                    instrumentoCertificadoSpDTO.Nemotecnico, instrumentoCertificadoSpDTO.ValorNominalInicial, instrumentoCertificadoSpDTO.CodigoIsin,
                    instrumentoCertificadoSpDTO.ValorNominalSbs, instrumentoCertificadoSpDTO.PorcentajeRatioSuscripcion,
                    IdSecuencialFechaAcuerdo, IdSecuencialFechaInicioNegociacion,
                    IdSecuencialFechaCorte, IdSecuencialFechaFinNegociacion,
                    IdSecuencialFechaRegistro, instrumentoCertificadoSpDTO.MontoEmitido,
                    IdSecuencialFechaMontoEmitido, IdSecuencialFechaEntrega,
                    instrumentoCertificadoSpDTO.MontoColocado, IdSecuencialFechaMontoColocado,
                    instrumentoCertificadoSpDTO.IndTipoCustodia, instrumentoCertificadoSpDTO.IndTipoUnidadEmision,
                    instrumentoCertificadoSpDTO.TieneMandato, instrumentoCertificadoSpDTO.LoginActualizacion);

                instrumentoCertificadoSp.IdCertificadoSuscripcionPreferente = persistedCertificadoSuscripcionPreferente.IdCertificadoSuscripcionPreferente;
                iInstrumentoCertificadoSuscripcionPreferenteRepository.Merge(persistedCertificadoSuscripcionPreferente, instrumentoCertificadoSp);
                iInstrumentoCertificadoSuscripcionPreferenteRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public InstrumentoCertificadoSuscripcionPreferenteDTO GetByIdCertificadoSP(int idInstrumento, int idInstrumentoCertificadoSuscripcionPreferente)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.CertificadoSuscripcionPreferente = iInstrumentoCertificadoSuscripcionPreferenteRepository.Get(idInstrumentoCertificadoSuscripcionPreferente);

            if (instrumento.CertificadoSuscripcionPreferente == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string SecuencialFechaAcuerdo = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoSuscripcionPreferente.IdSecuencialFechaAcuerdo);
            string SecuencialFechaCorte = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoSuscripcionPreferente.IdSecuencialFechaCorte);
            string SecuencialFechaRegistro = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoSuscripcionPreferente.IdSecuencialFechaRegistro);
            string SecuencialFechaEntrega = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoSuscripcionPreferente.IdSecuencialFechaEntrega);
            string SecuencialFechaInicioNegociacion = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoSuscripcionPreferente.IdSecuencialFechaInicioNegociacion);
            string SecuencialFechaFinNegociacion = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoSuscripcionPreferente.IdSecuencialFechaFinNegociacion);
            string SecuencialFechaMontoEmitido = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoSuscripcionPreferente.IdSecuencialFechaMontoEmitido);
            string SecuencialFechaMontoColocado = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoSuscripcionPreferente.IdSecuencialFechaMontoColocado);

            instrumento.CertificadoSuscripcionPreferente.SecuencialFechaAcuerdo = SecuencialFechaAcuerdo;
            instrumento.CertificadoSuscripcionPreferente.SecuencialFechaCorte = SecuencialFechaCorte;
            instrumento.CertificadoSuscripcionPreferente.SecuencialFechaRegistro = SecuencialFechaRegistro;
            instrumento.CertificadoSuscripcionPreferente.SecuencialFechaEntrega = SecuencialFechaEntrega;
            instrumento.CertificadoSuscripcionPreferente.SecuencialFechaInicioNegociacion = SecuencialFechaInicioNegociacion;
            instrumento.CertificadoSuscripcionPreferente.SecuencialFechaFinNegociacion = SecuencialFechaFinNegociacion;
            instrumento.CertificadoSuscripcionPreferente.SecuencialFechaMontoEmitido = SecuencialFechaMontoEmitido;
            instrumento.CertificadoSuscripcionPreferente.SecuencialFechaMontoColocado = SecuencialFechaMontoColocado;

            return instrumento.ProjectedAs<InstrumentoCertificadoSuscripcionPreferenteDTO>();

        }
        public string AddNewCertificadoSP(InstrumentoCertificadoSuscripcionPreferenteDTO instrumentoCertificadoSpDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoCertificadoSpDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);

                VerifyNombreInstrumentoIsUnique(instrumentoCertificadoSpDTO.CodigoSbs, instrumentoCertificadoSpDTO.IdInstrumento);
                VerifyInstrumentoCertificadoSuscripcionPreferenteIsUnique(instrumentoCertificadoSpDTO);

                TipoInstrumentoDTO tipoInstrumentoDTO = iTipoInstrumentoAppService.GetById(instrumentoCertificadoSpDTO.IdTipoInstrumento.Value);
                int IdSecuencialFechaAcuerdo = (instrumentoCertificadoSpDTO.SecuencialFechaAcuerdo.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaAcuerdo);
                int IdSecuencialFechaInicioNegociacion = (instrumentoCertificadoSpDTO.SecuencialFechaInicioNegociacion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaInicioNegociacion);
                int IdSecuencialFechaCorte = (instrumentoCertificadoSpDTO.SecuencialFechaCorte.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaCorte);
                int IdSecuencialFechaFinNegociacion = (instrumentoCertificadoSpDTO.SecuencialFechaFinNegociacion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaFinNegociacion);
                int IdSecuencialFechaRegistro = (instrumentoCertificadoSpDTO.SecuencialFechaRegistro.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaRegistro);
                int IdSecuencialFechaMontoEmitido = (instrumentoCertificadoSpDTO.SecuencialFechaMontoEmitido.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaMontoEmitido);
                int IdSecuencialFechaEntrega = (instrumentoCertificadoSpDTO.SecuencialFechaEntrega.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaEntrega);
                int IdSecuencialFechaMontoColocado = (instrumentoCertificadoSpDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoSpDTO.SecuencialFechaMontoColocado);

                instrumentoCertificadoSpDTO.NombreInstrumento = string.Format("{0} - {1}", tipoInstrumentoDTO.NombreSbsTipoInstrumento, instrumentoCertificadoSpDTO.NombreInstrumento);
                Instrumento instrumento = new Instrumento(instrumentoCertificadoSpDTO.IdTipoInstrumento, instrumentoCertificadoSpDTO.NombreInstrumento,
                    instrumentoCertificadoSpDTO.CodigoSbs, instrumentoCertificadoSpDTO.IdMoneda, instrumentoCertificadoSpDTO.IdEmisor,
                    tipoInstrumentoDTO.IdGrupoInstrumento, instrumentoCertificadoSpDTO.IndCategoria, instrumentoCertificadoSpDTO.IndFamilia,
                    indActividad, instrumentoCertificadoSpDTO.IdClasificacionRiesgo, instrumentoCertificadoSpDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                SaveInstrumento(instrumento);

                InstrumentoCertificadoSuscripcionPreferente instrumentoCertificadoSp = new InstrumentoCertificadoSuscripcionPreferente(
                    instrumento.IdInstrumento, instrumentoCertificadoSpDTO.IdAccion,
                    instrumentoCertificadoSpDTO.Nemotecnico, instrumentoCertificadoSpDTO.ValorNominalInicial, instrumentoCertificadoSpDTO.CodigoIsin,
                    instrumentoCertificadoSpDTO.ValorNominalSbs, instrumentoCertificadoSpDTO.PorcentajeRatioSuscripcion,
                    IdSecuencialFechaAcuerdo, IdSecuencialFechaInicioNegociacion,
                    IdSecuencialFechaCorte, IdSecuencialFechaFinNegociacion,
                    IdSecuencialFechaRegistro, instrumentoCertificadoSpDTO.MontoEmitido,
                    IdSecuencialFechaMontoEmitido, IdSecuencialFechaEntrega,
                    instrumentoCertificadoSpDTO.MontoColocado, IdSecuencialFechaMontoColocado,
                    instrumentoCertificadoSpDTO.IndTipoCustodia, instrumentoCertificadoSpDTO.IndTipoUnidadEmision,
                    instrumentoCertificadoSpDTO.TieneMandato, instrumentoCertificadoSpDTO.LoginActualizacion);

                SaveInstrumentoCertificadoSuscripcionPreferente(instrumentoCertificadoSp);
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_RegistrarElemento;
        }
        public string RemoveCertificadoSP(int idInstrumento, int idInstrumentoCertificadoSuscripcionPreferente)
        {
            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);

            InstrumentoCertificadoSuscripcionPreferente persistedCertificadoSuscripcionPreferente = iInstrumentoCertificadoSuscripcionPreferenteRepository.Get(idInstrumentoCertificadoSuscripcionPreferente);

            if (persisted == null || persistedCertificadoSuscripcionPreferente == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_CertificadoSuscripcionPreferente_Tiene_Dependencias));

            iInstrumentoCertificadoSuscripcionPreferenteRepository.Remove(persistedCertificadoSuscripcionPreferente);
            iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
            iInstrumentoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_EliminarElemento;
        }
        public string AnnulCertificadoSP(InstrumentoCertificadoSuscripcionPreferenteDTO instrumentoCertificadoSpDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoCertificadoSpDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoCertificadoSpDTO.IdInstrumento);
                InstrumentoCertificadoSuscripcionPreferente persistedCertificadoSuscripcionPreferente = iInstrumentoCertificadoSuscripcionPreferenteRepository.Get(instrumentoCertificadoSpDTO.IdCertificadoSuscripcionPreferente);
                if (persisted == null || persistedCertificadoSuscripcionPreferente == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int idAnulado = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

                Instrumento current = new Instrumento(instrumentoCertificadoSpDTO.ComentarioAnulacion,
                    instrumentoCertificadoSpDTO.LoginActualizacion);

                persisted.IndActividad = idAnulado;
                persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedCertificadoSuscripcionPreferente.LoginActualizacion = current.LoginActualizacion;
                persistedCertificadoSuscripcionPreferente.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedCertificadoSuscripcionPreferente.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoCertificadoSuscripcionPreferenteRepository.Merge(persistedCertificadoSuscripcionPreferente, persistedCertificadoSuscripcionPreferente);
                iInstrumentoCertificadoSuscripcionPreferenteRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public string ActiveCertificadoSP(InstrumentoCertificadoSuscripcionPreferenteDTO instrumentoCertificadoSpDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoCertificadoSpDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoCertificadoSpDTO.IdInstrumento);
                InstrumentoCertificadoSuscripcionPreferente persistedCertificadoSuscripcionPreferente = iInstrumentoCertificadoSuscripcionPreferenteRepository.Get(instrumentoCertificadoSpDTO.IdCertificadoSuscripcionPreferente);
                if (persisted == null || persistedCertificadoSuscripcionPreferente == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoCertificadoSpDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoCertificadoSpDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;
                Instrumento current = new Instrumento(instrumentoCertificadoSpDTO.ComentarioHabilitacion,
                    instrumentoCertificadoSpDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedCertificadoSuscripcionPreferente.LoginActualizacion = current.LoginActualizacion;
                persistedCertificadoSuscripcionPreferente.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedCertificadoSuscripcionPreferente.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoCertificadoSuscripcionPreferenteRepository.Merge(persistedCertificadoSuscripcionPreferente, persistedCertificadoSuscripcionPreferente);
                iInstrumentoCertificadoSuscripcionPreferenteRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensaje;
        }
        #endregion

        #region InstrumentoFondoMutuoAppService Members
        public List<InstrumentoFondoMutuoDTO> GetAllInstrumentoFondoMutuoActivoAndHabilitado()
        {
            var indHabilitado = iIndicadorRepository.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);
            var indVigente = iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            var fondosmutuos = iInstrumentoFondoMutuoRepository.GetAllForEstadoAndHabilitacion(indVigente, indHabilitado);
            return fondosmutuos.Select(x => new InstrumentoFondoMutuoDTO
            {
                IdInstrumento = x.IdInstrumento,
                IdFondoMutuo = x.IdFondoMutuo,
                CodigoSbs = x.Instrumento.CodigoSbs,
                CodigoIsin = x.CodigoIsin
            }).ToList();
        }
        public InstrumentoFondoMutuoPagedDTO GetFilteredDataFondoMutuo(string codigoSbs, int idGrupoInstrumento, int idTipoInstrumento, int idEmisor, int idMoneda, int indActividad, int indHabilitacion, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;

            InstrumentoFondoMutuoPagedDTO response = iInstrumentoDataRepository.GetFilteredDataFondoMutuo(codigoSbs, idGrupoInstrumento, idTipoInstrumento, idEmisor, idMoneda, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);
            foreach (InstrumentoFondoMutuoListadoDTO item in response.ListaInstrumentoFondoMutuo)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }
            return response;
        }
        public string UpdateInstrumentoFondoMutuo(InstrumentoFondoMutuoDTO instrumentoFondoMutuoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFondoMutuoDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoMutuoDTO"));

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoFondoMutuoDTO.IdInstrumento);
                InstrumentoFondoMutuo persistedFondoMutuo = iInstrumentoFondoMutuoRepository.Get(instrumentoFondoMutuoDTO.IdFondoMutuo);

                if (persisted == null || persistedFondoMutuo == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                VerifyNombreInstrumentoIsUnique(instrumentoFondoMutuoDTO.CodigoSbs, instrumentoFondoMutuoDTO.IdInstrumento);
                VerifyInstrumentoFondoMutuoIsUnique(instrumentoFondoMutuoDTO);

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoFondoMutuoDTO.IndHabilitacionRiesgo);
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoFondoMutuoDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                int IdSecuencialFechaInicio = (instrumentoFondoMutuoDTO.SecuencialFechaInicio.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFondoMutuoDTO.SecuencialFechaInicio);
                int IdSecuencialFechaCierreFiscal = (instrumentoFondoMutuoDTO.SecuencialFechaCierreFiscal.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFondoMutuoDTO.SecuencialFechaCierreFiscal);
                int? IdSecuencialFechaVencimiento = (instrumentoFondoMutuoDTO.SecuencialFechaVencimiento.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoFondoMutuoDTO.SecuencialFechaVencimiento);

                Instrumento current = new Instrumento(instrumentoFondoMutuoDTO.IdTipoInstrumento, instrumentoFondoMutuoDTO.NombreInstrumento, instrumentoFondoMutuoDTO.CodigoSbs,
                    instrumentoFondoMutuoDTO.IdMoneda, instrumentoFondoMutuoDTO.IdEmisor, IdGrupoInstrumento, instrumentoFondoMutuoDTO.IndCategoria,
                    instrumentoFondoMutuoDTO.IndFamilia, indActividad, instrumentoFondoMutuoDTO.IdClasificacionRiesgo, instrumentoFondoMutuoDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, current);
                iInstrumentoRepository.UnitOfWork.Commit();

                var indicadorSinCriterio = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.CriterioFondoMutuo && x.IdIndicador == (int)eCriterioFondoMutuo.SinCriterios).FirstOrDefault();
                var indicadorConCriterio = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.CriterioFondoMutuo && x.IdIndicador == (int)eCriterioFondoMutuo.ConCriterios).FirstOrDefault();
                var indicadorOpcionRebates = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.OpcionDistribucion && x.IdIndicador == (int)eOpcionDistribucion.Rebates).FirstOrDefault();

                if (indicadorSinCriterio == null || indicadorConCriterio == null)
                    throw new Exception("El indicador Criterio Fondo Mutuo Con/Sin Criterios no existe");

                if (persistedFondoMutuo.IndCriterioFondoMutuo == indicadorConCriterio.Id && instrumentoFondoMutuoDTO.IndOpcionDistribucion != indicadorOpcionRebates.Id)
                    throw new Exception("No se puede modificar la opción distribución por tener rebates asociados.");

                InstrumentoFondoMutuo instrumentoFondoMutuo = new InstrumentoFondoMutuo(instrumentoFondoMutuoDTO.IdInstrumento, instrumentoFondoMutuoDTO.NombreFondo,
                    instrumentoFondoMutuoDTO.Nemotecnico, instrumentoFondoMutuoDTO.TieneEtfs, instrumentoFondoMutuoDTO.TieneMonedaDual, instrumentoFondoMutuoDTO.IdMonedaDual,
                    instrumentoFondoMutuoDTO.TieneMandato, IdSecuencialFechaInicio, IdSecuencialFechaCierreFiscal,
                    instrumentoFondoMutuoDTO.TieneFechaVencimiento, IdSecuencialFechaVencimiento, instrumentoFondoMutuoDTO.IndTipoCustodia,
                    instrumentoFondoMutuoDTO.ClaseCuota, instrumentoFondoMutuoDTO.FormaPago, instrumentoFondoMutuoDTO.IndOpcionDistribucion, instrumentoFondoMutuoDTO.IndRegionEmision,
                    instrumentoFondoMutuoDTO.IndPaisEmision, instrumentoFondoMutuoDTO.IndFocoGeograficoEmision, instrumentoFondoMutuoDTO.IndClase,
                    instrumentoFondoMutuoDTO.IdSecuencialFechaTransferencia, instrumentoFondoMutuoDTO.Hora, instrumentoFondoMutuoDTO.IndTipoUnidadEmision,
                    instrumentoFondoMutuoDTO.MontoEmitido, instrumentoFondoMutuoDTO.MontoColocado, instrumentoFondoMutuoDTO.ValorNominalInicial, instrumentoFondoMutuoDTO.ValorNominalSbs,
                    instrumentoFondoMutuoDTO.Comentario, instrumentoFondoMutuoDTO.ClasificacionMicropal, instrumentoFondoMutuoDTO.CodigoIsin, instrumentoFondoMutuoDTO.CodigoCusip,
                    instrumentoFondoMutuoDTO.CodigoBloomberg, instrumentoFondoMutuoDTO.IndiceBenchmark, instrumentoFondoMutuoDTO.LoginActualizacion, instrumentoFondoMutuoDTO.IdMonedaUnidadMonto,
                    instrumentoFondoMutuoDTO.IndCriterioFondoMutuo != null ? instrumentoFondoMutuoDTO.IndCriterioFondoMutuo.Value : indicadorSinCriterio.Id,

                    instrumentoFondoMutuoDTO.IndCargaInversionIndirecta);

                instrumentoFondoMutuo.IdFondoMutuo = persistedFondoMutuo.IdFondoMutuo;
                iInstrumentoFondoMutuoRepository.Merge(persistedFondoMutuo, instrumentoFondoMutuo);
                iInstrumentoFondoMutuoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public InstrumentoFondoMutuoDTO GetByIdInstrumentoFondoMutuo(int idInstrumento, int IdFondoMutuo)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.FondoMutuo = iInstrumentoFondoMutuoRepository.Get(IdFondoMutuo);

            if (instrumento.FondoMutuo == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string SecuencialFechaInicio = Helper.ConvertIdFechaToFechaString(instrumento.FondoMutuo.IdSecuencialFechaInicio);
            string SecuencialFechaVencimiento = instrumento.FondoMutuo.IdSecuencialFechaVencimiento == null ? string.Empty : Helper.ConvertIdFechaToFechaString(instrumento.FondoMutuo.IdSecuencialFechaVencimiento.Value);
            string SecuencialFechaCierreFiscal = Helper.ConvertIdFechaToFechaString(instrumento.FondoMutuo.IdSecuencialFechaCierreFiscal);

            instrumento.FondoMutuo.SecuencialFechaInicio = SecuencialFechaInicio;
            instrumento.FondoMutuo.SecuencialFechaVencimiento = SecuencialFechaVencimiento;
            instrumento.FondoMutuo.SecuencialFechaCierreFiscal = SecuencialFechaCierreFiscal;

            return instrumento.ProjectedAs<InstrumentoFondoMutuoDTO>();
        }

        public List<InstrumentoFondoMutuoDTO> GetAllFondoMutuoEnRebate()
        {
            var rebateId = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive(
                (int)eIndicador.OpcionDistribucion, (int)eOpcionDistribucion.Rebates).Id;
            var instrumentoFondoMutuoDb = iInstrumentoFondoMutuoRepository
                .GetFiltered(x => x.IndOpcionDistribucion == rebateId).ToList();

            foreach (var x in instrumentoFondoMutuoDb)
            {
                var instrumentoDb = iInstrumentoRepository
                    .GetFiltered(y => y.IdInstrumento == x.IdInstrumento)
                    .FirstOrDefault();

                var instrumentoFondoMutuoDTO = new InstrumentoFondoMutuoDTO()
                {
                    ClaseCuota = x.ClaseCuota,
                    ClasificacionMicropal = x.ClasificacionMicropal,
                    CodigoBloomberg = x.CodigoBloomberg,
                    CodigoCusip = x.CodigoCusip,
                    CodigoIsin = x.CodigoIsin,
                    CodigoSbs = x.Instrumento.CodigoSbs,
                    CodigoSbsGenerated = string.Empty,
                    Comentario = x.Comentario,
                    ComentarioAnulacion = instrumentoDb != null ? instrumentoDb.ComentarioAnulacion : string.Empty,
                    ComentarioHabilitacion = instrumentoDb != null ? instrumentoDb.ComentarioHabilitacion : string.Empty,
                    FechaHoraActualizacion = x.FechaHoraActualizacion,
                    FechaHora = string.Empty,
                    FormaPago = x.FormaPago,
                    Hora = x.Hora,
                    IdClasificacionRiesgo = instrumentoDb != null ? instrumentoDb.ClasificacionRiesgo.IdClasificacionRiesgo : 0,
                    IdEmisor = instrumentoDb != null ? instrumentoDb.IdEmisor : 0,
                    IdFondoMutuo = x.IdFondoMutuo,
                    IdGrupoInstrumento = instrumentoDb != null ? instrumentoDb.IdGrupoInstrumento : 0,
                    IdHabilitacion = instrumentoDb != null ? instrumentoDb.IdHabilitacion : 0,
                    IdInstrumento = x.IdInstrumento,
                    IdMoneda = instrumentoDb != null ? instrumentoDb.IdMoneda : 0,
                    IdMonedaDual = x.IdMonedaDual,
                    IdMonedaUnidadMonto = x.IdMonedaUnidadMonto,
                    IdSecuencialFechaCierreFiscal = x.IdSecuencialFechaCierreFiscal,
                    IdSecuencialFechaInicio = x.IdSecuencialFechaInicio,
                    IdSecuencialFechaTransferencia = 0,
                    IdSecuencialFechaVencimiento = x.IdSecuencialFechaVencimiento.HasValue ? x.IdSecuencialFechaVencimiento.Value : 0,
                    IdTipoInstrumento = instrumentoDb != null ? instrumentoDb.TipoInstrumento.IdTipoInstrumento : 0,
                    IndActividad = instrumentoDb != null ? instrumentoDb.IndActividad : 0,
                    IndCargaInversionIndirecta = x.IndCargaInversionIndirecta,
                    IndCategoria = instrumentoDb != null ? instrumentoDb.IndCategoria : 0,
                    IndClase = x.IndClase,
                    IndCriterioFondoMutuo = x.IndCriterioFondoMutuo,
                    IndFamilia = instrumentoDb != null ? instrumentoDb.IndFamilia : 0,
                    IndFocoGeograficoEmision = x.IndFocoGeograficoEmision,
                    IndHabilitacionRiesgo = instrumentoDb != null ? instrumentoDb.IndHabilitacionRiesgo : 0,
                    IndiceBenchmark = x.IndiceBenchmark,
                    IndOpcionDistribucion = x.IndOpcionDistribucion,
                    IndPaisEmision = x.IndPaisEmision,
                    IndRegionEmision = x.IndRegionEmision,
                    IndTipoCustodia = x.IndTipoCustodia,
                    IndTipoUnidadEmision = x.IndTipoUnidadEmision,
                    LoginActualizacion = x.LoginActualizacion,
                    MontoColocado = x.MontoColocado,
                    MontoEmitido = x.MontoEmitido,
                    Nemotecnico = x.Nemotecnico,
                    NombreFondo = x.NombreFondo,
                    NombreInstrumento = instrumentoDb != null ? instrumentoDb.NombreInstrumento : string.Empty,
                    SecuencialFechaCierreFiscal = x.SecuencialFechaCierreFiscal,
                    SecuencialFechaInicio = x.SecuencialFechaInicio,
                    SecuencialFechaTransferencia = string.Empty,
                    SecuencialFechaVencimiento = x.SecuencialFechaVencimiento,
                    TieneEtfs = x.TieneEtfs,
                    TieneFechaVencimiento = x.TieneFechaVencimiento,
                    TieneMandato = x.TieneMandato,
                    TieneMonedaDual = x.TieneMonedaDual,
                    ValorNominalInicial = x.ValorNominalInicial,
                    ValorNominalSbs = x.ValorNominalSbs
                };
            }



            return instrumentoFondoMutuoDb.Select(x => new InstrumentoFondoMutuoDTO()
            {
                ClaseCuota = x.ClaseCuota,
                ClasificacionMicropal = x.ClasificacionMicropal,
                CodigoBloomberg = x.CodigoBloomberg,
                CodigoCusip = x.CodigoCusip,
                CodigoIsin = x.CodigoIsin,
                CodigoSbs = x.Instrumento.CodigoSbs,
                CodigoSbsGenerated = string.Empty,
                Comentario = x.Comentario,
                ComentarioAnulacion = x.Instrumento != null ? x.Instrumento.ComentarioAnulacion : string.Empty,
                ComentarioHabilitacion = x.Instrumento != null ? x.Instrumento.ComentarioHabilitacion : string.Empty,
                FechaHoraActualizacion = x.FechaHoraActualizacion,
                FechaHora = string.Empty,
                FormaPago = x.FormaPago,
                Hora = x.Hora,
                IdClasificacionRiesgo = x.Instrumento != null ? x.Instrumento.ClasificacionRiesgo.IdClasificacionRiesgo : 0,
                IdEmisor = x.Instrumento != null ? x.Instrumento.IdEmisor : 0,
                IdFondoMutuo = x.IdFondoMutuo,
                IdGrupoInstrumento = x.Instrumento != null ? x.Instrumento.IdGrupoInstrumento : 0,
                IdHabilitacion = x.Instrumento != null ? x.Instrumento.IdHabilitacion : 0,
                IdInstrumento = x.IdInstrumento,
                IdMoneda = x.Instrumento != null ? x.Instrumento.IdMoneda : 0,
                IdMonedaDual = x.IdMonedaDual,
                IdMonedaUnidadMonto = x.IdMonedaUnidadMonto,
                IdSecuencialFechaCierreFiscal = x.IdSecuencialFechaCierreFiscal,
                IdSecuencialFechaInicio = x.IdSecuencialFechaInicio,
                IdSecuencialFechaTransferencia = 0,
                IdSecuencialFechaVencimiento = x.IdSecuencialFechaVencimiento.HasValue ? x.IdSecuencialFechaVencimiento.Value : 0,
                IdTipoInstrumento = x.Instrumento != null ? x.Instrumento.TipoInstrumento.IdTipoInstrumento : 0,
                IndActividad = x.Instrumento != null ? x.Instrumento.IndActividad : 0,
                IndCargaInversionIndirecta = x.IndCargaInversionIndirecta,
                IndCategoria = x.Instrumento != null ? x.Instrumento.IndCategoria : 0,
                IndClase = x.IndClase,
                IndCriterioFondoMutuo = x.IndCriterioFondoMutuo,
                IndFamilia = x.Instrumento != null ? x.Instrumento.IndFamilia : 0,
                IndFocoGeograficoEmision = x.IndFocoGeograficoEmision,
                IndHabilitacionRiesgo = x.Instrumento != null ? x.Instrumento.IndHabilitacionRiesgo : 0,
                IndiceBenchmark = x.IndiceBenchmark,
                IndOpcionDistribucion = x.IndOpcionDistribucion,
                IndPaisEmision = x.IndPaisEmision,
                IndRegionEmision = x.IndRegionEmision,
                IndTipoCustodia = x.IndTipoCustodia,
                IndTipoUnidadEmision = x.IndTipoUnidadEmision,
                LoginActualizacion = x.LoginActualizacion,
                MontoColocado = x.MontoColocado,
                MontoEmitido = x.MontoEmitido,
                Nemotecnico = x.Nemotecnico,
                NombreFondo = x.NombreFondo,
                NombreInstrumento = x.Instrumento != null ? x.Instrumento.NombreInstrumento : string.Empty,
                SecuencialFechaCierreFiscal = x.SecuencialFechaCierreFiscal,
                SecuencialFechaInicio = x.SecuencialFechaInicio,
                SecuencialFechaTransferencia = string.Empty,
                SecuencialFechaVencimiento = x.SecuencialFechaVencimiento,
                TieneEtfs = x.TieneEtfs,
                TieneFechaVencimiento = x.TieneFechaVencimiento,
                TieneMandato = x.TieneMandato,
                TieneMonedaDual = x.TieneMonedaDual,
                ValorNominalInicial = x.ValorNominalInicial,
                ValorNominalSbs = x.ValorNominalSbs
            }).ToList();
        }

        public string AddNewInstrumentoFondoMutuo(InstrumentoFondoMutuoDTO instrumentoFondoMutuoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFondoMutuoDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);

                int IdSecuencialFechaInicio = (instrumentoFondoMutuoDTO.SecuencialFechaInicio.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFondoMutuoDTO.SecuencialFechaInicio);
                int IdSecuencialFechaCierreFiscal = (instrumentoFondoMutuoDTO.SecuencialFechaCierreFiscal.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFondoMutuoDTO.SecuencialFechaCierreFiscal);
                int? IdSecuencialFechaVencimiento = (instrumentoFondoMutuoDTO.SecuencialFechaVencimiento.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoFondoMutuoDTO.SecuencialFechaVencimiento);
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoFondoMutuoDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                VerifyNombreInstrumentoIsUnique(instrumentoFondoMutuoDTO.CodigoSbs, instrumentoFondoMutuoDTO.IdInstrumento);
                VerifyInstrumentoFondoMutuoIsUnique(instrumentoFondoMutuoDTO);

                Instrumento instrumento = new Instrumento(instrumentoFondoMutuoDTO.IdTipoInstrumento, instrumentoFondoMutuoDTO.NombreInstrumento, instrumentoFondoMutuoDTO.CodigoSbs,
                    instrumentoFondoMutuoDTO.IdMoneda, instrumentoFondoMutuoDTO.IdEmisor, IdGrupoInstrumento, instrumentoFondoMutuoDTO.IndCategoria,
                    instrumentoFondoMutuoDTO.IndFamilia, indActividad, instrumentoFondoMutuoDTO.IdClasificacionRiesgo, instrumentoFondoMutuoDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                SaveInstrumento(instrumento);

                var indicadorCriterio = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.CriterioFondoMutuo && x.IdIndicador == (int)eCriterioFondoMutuo.SinCriterios).FirstOrDefault();
                if (indicadorCriterio == null)
                    throw new Exception("El indicador Criterio Fondo Mutuo Con Criterios no existe");


                InstrumentoFondoMutuo instrumentoFondoMutuo = new InstrumentoFondoMutuo(instrumento.IdInstrumento, instrumentoFondoMutuoDTO.NombreFondo,
                    instrumentoFondoMutuoDTO.Nemotecnico, instrumentoFondoMutuoDTO.TieneEtfs, instrumentoFondoMutuoDTO.TieneMonedaDual, instrumentoFondoMutuoDTO.IdMonedaDual,
                    instrumentoFondoMutuoDTO.TieneMandato, IdSecuencialFechaInicio, IdSecuencialFechaCierreFiscal,
                    instrumentoFondoMutuoDTO.TieneFechaVencimiento, IdSecuencialFechaVencimiento, instrumentoFondoMutuoDTO.IndTipoCustodia,
                    instrumentoFondoMutuoDTO.ClaseCuota, instrumentoFondoMutuoDTO.FormaPago, instrumentoFondoMutuoDTO.IndOpcionDistribucion, instrumentoFondoMutuoDTO.IndRegionEmision,
                    instrumentoFondoMutuoDTO.IndPaisEmision, instrumentoFondoMutuoDTO.IndFocoGeograficoEmision, instrumentoFondoMutuoDTO.IndClase,
                    instrumentoFondoMutuoDTO.IdSecuencialFechaTransferencia, instrumentoFondoMutuoDTO.Hora, instrumentoFondoMutuoDTO.IndTipoUnidadEmision,
                    instrumentoFondoMutuoDTO.MontoEmitido, instrumentoFondoMutuoDTO.MontoColocado, instrumentoFondoMutuoDTO.ValorNominalInicial, instrumentoFondoMutuoDTO.ValorNominalSbs,
                    instrumentoFondoMutuoDTO.Comentario, instrumentoFondoMutuoDTO.ClasificacionMicropal, instrumentoFondoMutuoDTO.CodigoIsin, instrumentoFondoMutuoDTO.CodigoCusip,
                    instrumentoFondoMutuoDTO.CodigoBloomberg, instrumentoFondoMutuoDTO.IndiceBenchmark, instrumentoFondoMutuoDTO.LoginActualizacion, instrumentoFondoMutuoDTO.IdMonedaUnidadMonto,
                    indicadorCriterio.Id,

                    instrumentoFondoMutuoDTO.IndCargaInversionIndirecta);

                SaveInstrumentoFondoMutuo(instrumentoFondoMutuo);
                transactionScope.Complete();
            }

            return mensajeGenericoES.exito_RegistrarElemento;
        }
        public string RemoveInstrumentoFondoMutuo(int idInstrumento, int IdFondoMutuo)
        {

            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);

            InstrumentoFondoMutuo persistedFondoMutuo = iInstrumentoFondoMutuoRepository.Get(IdFondoMutuo);

            CriterioCalculoRebate[] persistedCriterioCalculoRebateFondoMutuo = iCriterioCalculoRebateRepository.GetFiltered(x => x.IdFondoMutuo == IdFondoMutuo).ToArray();

            if (persisted == null || persistedFondoMutuo == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_FondoMutuo_Tiene_Dependencias));


            var indicadorConCriterio = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.CriterioFondoMutuo && x.IdIndicador == (int)eCriterioFondoMutuo.ConCriterios).FirstOrDefault();
            var indicadorOpcionRebates = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.OpcionDistribucion && x.IdIndicador == (int)eOpcionDistribucion.Rebates).FirstOrDefault();

            if (persistedFondoMutuo.IndCriterioFondoMutuo == indicadorConCriterio.Id && persistedFondoMutuo.IndOpcionDistribucion != indicadorOpcionRebates.Id)
                throw new Exception("No se puede eliminar por tener rebates asociados.");

            foreach (var item in persistedCriterioCalculoRebateFondoMutuo)
            {
                iCriterioCalculoRebateRepository.Remove(item);
            }

            iInstrumentoFondoMutuoRepository.Remove(persistedFondoMutuo);
            iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
            iInstrumentoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_EliminarElemento;
        }
        public string AnnulInstrumentoFondoMutuo(InstrumentoFondoMutuoDTO instrumentoFondoMutuoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFondoMutuoDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoFondoMutuoDTO.IdInstrumento);
                InstrumentoFondoMutuo persistedFondoMutuo = iInstrumentoFondoMutuoRepository.Get(instrumentoFondoMutuoDTO.IdFondoMutuo);

                if (persisted == null || persistedFondoMutuo == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int idAnulado = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

                Instrumento current = new Instrumento(instrumentoFondoMutuoDTO.ComentarioAnulacion,
                    instrumentoFondoMutuoDTO.LoginActualizacion);

                persisted.IndActividad = idAnulado;
                persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedFondoMutuo.LoginActualizacion = current.LoginActualizacion;
                persistedFondoMutuo.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedFondoMutuo.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoFondoMutuoRepository.Merge(persistedFondoMutuo, persistedFondoMutuo);
                iInstrumentoFondoMutuoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_AnularElemento;
        }
        public string ActiveInstrumentoFondoMutuo(InstrumentoFondoMutuoDTO instrumentoFondoMutuoDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFondoMutuoDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoFondoMutuoDTO.IdInstrumento);
                if (!iAnexoIRubroRepository.Any(p => p.AnexoIRubroDetalleInstrumento.Any(q => q.IdTipoInstrumento == persisted.TipoInstrumento.IdTipoInstrumento && q.IdEmisor == persisted.IdEmisor)))
                    throw new ApplicationException("No tiene detalle en el Anexo III");

                InstrumentoFondoMutuo persistedFondoMutuo = iInstrumentoFondoMutuoRepository.Get(instrumentoFondoMutuoDTO.IdFondoMutuo);

                if (persisted == null || persistedFondoMutuo == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoFondoMutuoDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoFondoMutuoDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;
                Instrumento current = new Instrumento(instrumentoFondoMutuoDTO.ComentarioHabilitacion,
                    instrumentoFondoMutuoDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedFondoMutuo.LoginActualizacion = current.LoginActualizacion;
                persistedFondoMutuo.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedFondoMutuo.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoFondoMutuoRepository.Merge(persistedFondoMutuo, persistedFondoMutuo);
                iInstrumentoFondoMutuoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensaje;
        }
        #endregion

        #region InstrumentoRentaFijaAppService Members
        public InstrumentoRentaFijaPagedDTO GetFilteredDataRentaFija(string codigoSbs, int idGrupoInstrumento, int tipoInstrumento, int idEmisor, int idMoneda, int indActividad, int indHabilitacion, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;

            InstrumentoRentaFijaPagedDTO response = iInstrumentoDataRepository.GetFilteredDataRentaFija(codigoSbs, idGrupoInstrumento, tipoInstrumento, idEmisor, idMoneda, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);
            foreach (InstrumentoRentaFijaListadoDTO item in response.ListaInstrumentoRentaFija)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }
            return response;
        }
        public string UpdateInstrumentoRentaFija(InstrumentoRentaFijaDTO instrumentoRentaFijaDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoRentaFijaDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoRentaFijaDTO"));

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoRentaFijaDTO.IdInstrumento);
                InstrumentoRentaFija persistedRentaFija = iInstrumentoRentaFijaRepository.Get(instrumentoRentaFijaDTO.IdRentaFija);

                if (persisted == null || persistedRentaFija == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoRentaFijaDTO.IndHabilitacionRiesgo);
                if (persistedRentaFija.IsGdn != null && persistedRentaFija.IsGdn.Value)
                {
                    persistedRentaFija.MontoEmitido = instrumentoRentaFijaDTO.MontoEmitido;
                    persistedRentaFija.MontoColocado = instrumentoRentaFijaDTO.MontoColocado;
                    persistedRentaFija.IdFechaMontoColocado = (instrumentoRentaFijaDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaMontoColocado);
                    persisted.IdClasificacionRiesgo = instrumentoRentaFijaDTO.IdClasificacionRiesgo;
                    persistedRentaFija.IndPaisEmision = instrumentoRentaFijaDTO.IndPaisEmision;
                    persisted.LoginActualizacion = instrumentoRentaFijaDTO.LoginActualizacion;
                    persisted.FechaHoraActualizacion = DateTime.Now;
                    persistedRentaFija.LoginActualizacion = instrumentoRentaFijaDTO.LoginActualizacion;
                    persistedRentaFija.FechaHoraActualizacion = DateTime.Now;
                    persisted.IndHabilitacionRiesgo = indHabilitacionRiesgo;
                    iInstrumentoRentaFijaRepository.UnitOfWork.Commit();
                    transactionScope.Complete();
                }
                else
                {
                    VerifyNombreInstrumentoIsUnique(instrumentoRentaFijaDTO.CodigoSbs, instrumentoRentaFijaDTO.IdInstrumento);
                    VerifyInstrumentoRentaFijaIsUnique(instrumentoRentaFijaDTO);

                    Instrumento persistedPadre = new Instrumento();
                    InstrumentoRentaFija persistedRentaFijaHija = new InstrumentoRentaFija();
                    string codigoSbsGenerated = "";
                    if (instrumentoRentaFijaDTO.Gdn != null)
                    {
                        if (instrumentoRentaFijaDTO.Gdn.IdInstrumentoGdn != 0)
                            persistedPadre = iInstrumentoRepository.Get(instrumentoRentaFijaDTO.Gdn.IdInstrumentoGdn);

                        persistedRentaFijaHija = iInstrumentoRentaFijaRepository.GetFiltered(x => x.IdRentaFija == instrumentoRentaFijaDTO.Gdn.IdRentaFijaGdn).FirstOrDefault();

                        codigoSbsGenerated = GeneratedCodigoSbsOnlyFirst7Digits(instrumentoRentaFijaDTO.Gdn.CodigoSbs, instrumentoRentaFijaDTO.Gdn.IdMoneda, instrumentoRentaFijaDTO.IdEmisor, instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento, 2);
                        VerifyNombreInstrumentoIsUnique(codigoSbsGenerated, instrumentoRentaFijaDTO.Gdn.IdInstrumentoGdn);
                        VerifyInstrumentoRentaFijaGdnIsUnique(instrumentoRentaFijaDTO.Gdn, instrumentoRentaFijaDTO.Gdn.IdRentaFijaGdn);
                    }


                    int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                    int indEstadoVigenciaCuponVigente = iIndicadorAppService.GetId((int)eIndicador.EstadoVigenciaCupon, (int)eEstadoVigenciaCupon.Vigente);
                    int indEstadoVigenciaCuponVencido = iIndicadorAppService.GetId((int)eIndicador.EstadoVigenciaCupon, (int)eEstadoVigenciaCupon.Vencido);
                    int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoRentaFijaDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                    int IdFechaEmision = (instrumentoRentaFijaDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaEmision);
                    int IdFechaVencimiento = (instrumentoRentaFijaDTO.SecuencialFechaVencimiento.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaVencimiento);
                    int IdFechaMontoColocado = (instrumentoRentaFijaDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaMontoColocado);
                    int? IdFechaIndicadorInteres = (instrumentoRentaFijaDTO.SecuencialFechaIndicadorInteres.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaIndicadorInteres);
                    int? IdFechaFinPeriodoGracia = (instrumentoRentaFijaDTO.SecuencialFechaFinPeriodoGracia.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaFinPeriodoGracia);

                    //IndicadorDTO indicadorInteres = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.IndicadorInteresesBonos, instrumentoRentaFijaDTO.IndTipoInteres);
                    //if (indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon))
                    //    instrumentoRentaFijaDTO.Cuponera = GenerarCeroCupon(instrumentoRentaFijaDTO.SecuencialFechaEmision, instrumentoRentaFijaDTO.SecuencialFechaVencimiento, instrumentoRentaFijaDTO.ValorNominalInicial, instrumentoRentaFijaDTO.IdMercado);

                    Instrumento current = new Instrumento(instrumentoRentaFijaDTO.IdTipoInstrumento, instrumentoRentaFijaDTO.NombreInstrumento, instrumentoRentaFijaDTO.CodigoSbs,
                        instrumentoRentaFijaDTO.IdMoneda, instrumentoRentaFijaDTO.IdEmisor, IdGrupoInstrumento, instrumentoRentaFijaDTO.IndCategoria,
                        instrumentoRentaFijaDTO.IndFamilia, indActividad, instrumentoRentaFijaDTO.IdClasificacionRiesgo, instrumentoRentaFijaDTO.LoginActualizacion,
                        indHabilitacionRiesgo);

                    current.IdInstrumento = persisted.IdInstrumento;
                    iInstrumentoRepository.Merge(persisted, current);
                    iInstrumentoRepository.UnitOfWork.Commit();

                    InstrumentoRentaFija instrumentoRentaFija = new InstrumentoRentaFija(instrumentoRentaFijaDTO.IdInstrumento, instrumentoRentaFijaDTO.Nemotecnico,
                        instrumentoRentaFijaDTO.CodigoIsin, instrumentoRentaFijaDTO.TieneMonedaDual, instrumentoRentaFijaDTO.IdMonedaDual,
                        IdFechaEmision, IdFechaVencimiento, IdFechaMontoColocado, IdFechaIndicadorInteres, instrumentoRentaFijaDTO.ValorNominalInicial,
                        instrumentoRentaFijaDTO.ValorNominalVigente, instrumentoRentaFijaDTO.ValorNominalSbs, instrumentoRentaFijaDTO.IndTipoUnidadEmision,
                        instrumentoRentaFijaDTO.IndTipoCustodia, instrumentoRentaFijaDTO.MontoEmitido, instrumentoRentaFijaDTO.MontoColocado, instrumentoRentaFijaDTO.IndTipoInteres,
                        instrumentoRentaFijaDTO.TieneMandato, instrumentoRentaFijaDTO.IndClasificadora1, instrumentoRentaFijaDTO.IndRatingClasificacion1,
                        instrumentoRentaFijaDTO.IndClasificadora2, instrumentoRentaFijaDTO.IndRatingClasificacion2, instrumentoRentaFijaDTO.Emision,
                        instrumentoRentaFijaDTO.Programa, instrumentoRentaFijaDTO.Serie, instrumentoRentaFijaDTO.IndRegionEmision, instrumentoRentaFijaDTO.IndPaisEmision,
                        instrumentoRentaFijaDTO.IndFocoGeograficoEmision, instrumentoRentaFijaDTO.IndClase, instrumentoRentaFijaDTO.IndTipoAmortizacion,
                        instrumentoRentaFijaDTO.IndPeriodoPago, instrumentoRentaFijaDTO.IndBaseCalculo, instrumentoRentaFijaDTO.NroCupones,
                        instrumentoRentaFijaDTO.IndIndexarInflacion, instrumentoRentaFijaDTO.PorcentajeTasaFija, instrumentoRentaFijaDTO.IndTasaFlotante,
                        instrumentoRentaFijaDTO.PorcentajeTasaFlotante, instrumentoRentaFijaDTO.IndTipoPeriodoGracia, IdFechaFinPeriodoGracia,
                        instrumentoRentaFijaDTO.IndOpcionalidad, instrumentoRentaFijaDTO.Observaciones, instrumentoRentaFijaDTO.LoginActualizacion, instrumentoRentaFijaDTO.IdMercado, instrumentoRentaFijaDTO.IdTipoTasa, false);

                    instrumentoRentaFija.IdRentaFija = persistedRentaFija.IdRentaFija;
                    iInstrumentoRentaFijaRepository.Merge(persistedRentaFija, instrumentoRentaFija);
                    iInstrumentoRentaFijaRepository.UnitOfWork.Commit();

                    List<InstrumentoRentaFijaCupon> cuponesRegistrados = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(persistedRentaFija.IdRentaFija)).ToList();

                    if (cuponesRegistrados.Any(x => x.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido ||
                                                    x.VencimientoRentaFijaCupon.Any(z => z.IndEstadoVencimiento == indEstadoVigenciaCuponVencido)))
                    {
                        var cuponesVigentes = cuponesRegistrados.FindAll(cr => cr.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVigente && !cr.VencimientoRentaFijaCupon.Any()).ToArray();
                        foreach (var cupon in cuponesVigentes)
                        {
                            iInstrumentoRentaFijaCuponRepository.Remove(cupon);
                        }
                        iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();

                        var cuponesVencidos = cuponesRegistrados.FindAll(cr => cr.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido || cr.VencimientoRentaFijaCupon.Any()).ToArray();

                        foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoRentaFijaDTO.Cuponera)
                            if (!cuponesVencidos.Any(x => x.NumeroCupon == cupon.NumeroCupon))
                                AddNewRentaFijaCupon(cupon, instrumentoRentaFija.IdRentaFija, instrumentoRentaFijaDTO.LoginActualizacion);
                    }
                    else
                    {
                        foreach (var cupon in cuponesRegistrados)
                        {
                            iInstrumentoRentaFijaCuponRepository.Remove(cupon);
                        }
                        iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();
                        foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoRentaFijaDTO.Cuponera)
                            AddNewRentaFijaCupon(cupon, instrumentoRentaFija.IdRentaFija, instrumentoRentaFijaDTO.LoginActualizacion);

                        foreach (var item in persisted.VariacionValorNominalVigente.ToArray())
                            persisted.VariacionValorNominalVigente.Remove(item);

                        var primerCuponVigente = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumentoRentaFija.IdRentaFija) &&
                                                                                                      irfc.IndEstadoVigenciaCupon != indEstadoVigenciaCuponVencido)
                                                                                 .OrderBy(x => x.FechaCorte)
                                                                                 .FirstOrDefault();

                        if (primerCuponVigente == null)
                            throw new ApplicationException("No existe un cupon vigente para este instrumento.");

                        persisted.VariacionValorNominalVigente.Add(new VariacionValorNominalVigente
                        {
                            IdSecuencialFecha = Helper.ConvertToIdFecha(primerCuponVigente.FechaInicio),
                            ValorNominalVigente = instrumentoRentaFija.ValorNominalVigente
                        });
                    }

                    if (!instrumentoRentaFijaDTO.Cuponera.Any(x => x.IdIndicador == (int)eEstadoVigenciaCupon.Vigente))
                        persisted.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vencido);
                    else
                        persisted.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);

                    var ultimoCupon = instrumentoRentaFijaDTO.Cuponera.OrderByDescending(x => Helper.ConvertFechaStringToIdFecha(x.FechaCorte)).FirstOrDefault();
                    if (ultimoCupon != null)
                        persistedRentaFija.IdFechaVencimiento = Helper.ConvertFechaStringToIdFecha(ultimoCupon.FechaCorte);

                    var cuponesVencidosGuardados = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(persistedRentaFija.IdRentaFija) &&
                                                                                                   irfc.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido).ToList();

                    persistedRentaFija.ValorNominalVigente = persistedRentaFija.ValorNominalInicial;
                    if (cuponesVencidosGuardados.Any())
                    {
                        var amortisacion = cuponesVencidosGuardados.Sum(x => x.ImporteAmortizacion);
                        persistedRentaFija.ValorNominalVigente = persistedRentaFija.ValorNominalInicial - amortisacion;
                    }


                    if (instrumentoRentaFijaDTO.Gdn != null)
                    {
                        codigoSbsGenerated = GeneratedCodigoSbsOnlyFirst7Digits(instrumentoRentaFijaDTO.Gdn.CodigoSbs, instrumentoRentaFijaDTO.Gdn.IdMoneda,
                            instrumentoRentaFijaDTO.IdEmisor, instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento, 2);

                        Instrumento currentHija = new Instrumento(instrumentoRentaFijaDTO.IdTipoInstrumento, instrumentoRentaFijaDTO.Gdn.Nombre,
                            codigoSbsGenerated, instrumentoRentaFijaDTO.Gdn.IdMoneda, instrumentoRentaFijaDTO.IdEmisor, IdGrupoInstrumento, instrumentoRentaFijaDTO.IndCategoria,
                            instrumentoRentaFijaDTO.IndFamilia, indActividad, instrumentoRentaFijaDTO.IdClasificacionRiesgo, instrumentoRentaFijaDTO.LoginActualizacion,
                            indHabilitacionRiesgo);

                        InstrumentoRentaFija instrumentoRentaFijaHija;
                        List<InstrumentoRentaFijaCupon> cuponesRegistradosHija;
                        if (persistedRentaFijaHija == null)
                        {//new
                            SaveInstrumento(currentHija);

                            int idFecha = Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.Gdn.Fecha);

                            instrumentoRentaFijaHija =
                                new InstrumentoRentaFija(instrumentoRentaFija, currentHija.IdInstrumento, instrumentoRentaFijaDTO.Gdn.Nombre, instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento,
                                    instrumentoRentaFijaDTO.Gdn.Nemotecnico, instrumentoRentaFijaDTO.ValorNominalInicial, instrumentoRentaFijaDTO.Gdn.IdMoneda, instrumentoRentaFijaDTO.Gdn.CodIsin,
                                    instrumentoRentaFijaDTO.Gdn.MontoEmitido, codigoSbsGenerated, idFecha, instrumentoRentaFijaDTO.Gdn.FactorConversion,
                                    instrumentoRentaFija.IdRentaFija, true);

                            SaveInstrumentoRentaFija(instrumentoRentaFijaHija);

                            cuponesRegistradosHija = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumentoRentaFijaHija.IdRentaFija)).ToList();

                            if (cuponesRegistradosHija.Any(x => x.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido ||
                                                                x.VencimientoRentaFijaCupon.Any(z => z.IndEstadoVencimiento == indEstadoVigenciaCuponVencido)))
                            {
                                var cuponesVigentesHija = cuponesRegistradosHija.FindAll(cr => cr.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVigente &&
                                                                                               !cr.VencimientoRentaFijaCupon.Any(z => z.IndEstadoVencimiento == indEstadoVigenciaCuponVencido)).ToArray();
                                foreach (var cupon in cuponesVigentesHija)
                                {
                                    iInstrumentoRentaFijaCuponRepository.Remove(cupon);
                                }
                                iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();

                                var cuponesVencidosHija = cuponesRegistradosHija.FindAll(cr => cr.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido || cr.VencimientoRentaFijaCupon.Any(z => z.IndEstadoVencimiento == indEstadoVigenciaCuponVencido)).ToArray();

                                foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoRentaFijaDTO.Cuponera)
                                    if (!cuponesVencidosHija.Any(x => x.NumeroCupon == cupon.NumeroCupon))
                                        AddNewRentaFijaCupon(cupon, instrumentoRentaFijaHija.IdRentaFija, instrumentoRentaFijaDTO.LoginActualizacion);
                            }
                            else
                            {
                                foreach (var cupon in cuponesRegistradosHija)
                                    iInstrumentoRentaFijaCuponRepository.Remove(cupon);

                                iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();

                                foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoRentaFijaDTO.Cuponera)
                                    AddNewRentaFijaCupon(cupon, instrumentoRentaFijaHija.IdRentaFija, instrumentoRentaFijaDTO.LoginActualizacion);
                            }

                            if (!instrumentoRentaFijaDTO.Cuponera.Any(x => x.IdIndicador == (int)eEstadoVigenciaCupon.Vigente))
                                currentHija.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vencido);
                            else
                                currentHija.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);

                            var ultimoCuponHijo = instrumentoRentaFijaDTO.Cuponera.OrderByDescending(x => Helper.ConvertFechaStringToIdFecha(x.FechaCorte)).FirstOrDefault();
                            if (ultimoCuponHijo != null)
                                instrumentoRentaFijaHija.IdFechaVencimiento = Helper.ConvertFechaStringToIdFecha(ultimoCuponHijo.FechaCorte);


                            var cuponesVencidosHijo = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumentoRentaFijaHija.IdRentaFija) &&
                                                                                                           irfc.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido).ToList();

                            instrumentoRentaFijaHija.ValorNominalVigente = instrumentoRentaFijaHija.ValorNominalInicial;
                            if (cuponesVencidosHijo.Any())
                            {
                                var amortisacion = cuponesVencidosHijo.Sum(x => x.ImporteAmortizacion);
                                instrumentoRentaFijaHija.ValorNominalVigente = instrumentoRentaFijaHija.ValorNominalInicial - amortisacion;
                            }

                            /*  var nacionalidad = iIndicadorRepository.Get(instrumentoRentaFijaDTO.Gdn.IndNacionalidadGdn);
                              var tipoinstrumento = iTipoInstrumentoRepository.FirstOrDefault(x => x.CodigoSbsTipoInstrumento == nacionalidad.ValorAuxChar1);
                              if (tipoinstrumento != null)
                                */
                            currentHija.IdTipoInstrumento = instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento;

                        }
                        else
                        {//merge 
                            currentHija.IdInstrumento = persistedPadre.IdInstrumento;
                            iInstrumentoRepository.Merge(persistedPadre, currentHija);
                            iInstrumentoRepository.UnitOfWork.Commit();

                            int idFecha = Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.Gdn.Fecha);

                            instrumentoRentaFijaHija =
                                new InstrumentoRentaFija(instrumentoRentaFija, currentHija.IdInstrumento, instrumentoRentaFijaDTO.Gdn.Nombre, instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento,
                                    instrumentoRentaFijaDTO.Gdn.Nemotecnico, instrumentoRentaFijaDTO.ValorNominalInicial, instrumentoRentaFijaDTO.Gdn.IdMoneda, instrumentoRentaFijaDTO.Gdn.CodIsin,
                                    instrumentoRentaFijaDTO.Gdn.MontoEmitido, codigoSbsGenerated, idFecha, instrumentoRentaFijaDTO.Gdn.FactorConversion,
                                    instrumentoRentaFija.IdRentaFija, true);


                            instrumentoRentaFijaHija.IdRentaFija = persistedRentaFijaHija.IdRentaFija;
                            iInstrumentoRentaFijaRepository.Merge(persistedRentaFijaHija, instrumentoRentaFijaHija);
                            iInstrumentoRentaFijaRepository.UnitOfWork.Commit();


                            cuponesRegistradosHija = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumentoRentaFijaHija.IdRentaFija)).ToList();

                            if (cuponesRegistradosHija.Any(x => x.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido ||
                                                                x.VencimientoRentaFijaCupon.Any(z => z.IndEstadoVencimiento == indEstadoVigenciaCuponVencido)))
                            {
                                var cuponesVigentesHija = cuponesRegistradosHija.FindAll(cr => cr.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVigente &&
                                                                                              !cr.VencimientoRentaFijaCupon.Any(z => z.IndEstadoVencimiento == indEstadoVigenciaCuponVencido)).ToArray();
                                foreach (var cupon in cuponesVigentesHija)
                                {
                                    iInstrumentoRentaFijaCuponRepository.Remove(cupon);
                                }
                                iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();

                                var cuponesVencidosHija = cuponesRegistradosHija.FindAll(cr => cr.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido || cr.VencimientoRentaFijaCupon.Any(z => z.IndEstadoVencimiento == indEstadoVigenciaCuponVencido)).ToArray();

                                foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoRentaFijaDTO.Cuponera)
                                    if (!cuponesVencidosHija.Any(x => x.NumeroCupon == cupon.NumeroCupon))
                                        AddNewRentaFijaCupon(cupon, instrumentoRentaFijaHija.IdRentaFija, instrumentoRentaFijaDTO.LoginActualizacion);
                            }
                            else
                            {
                                foreach (var cupon in cuponesRegistradosHija)
                                    iInstrumentoRentaFijaCuponRepository.Remove(cupon);

                                iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();

                                foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoRentaFijaDTO.Cuponera)
                                    AddNewRentaFijaCupon(cupon, instrumentoRentaFijaHija.IdRentaFija, instrumentoRentaFijaDTO.LoginActualizacion);
                            }

                            if (!instrumentoRentaFijaDTO.Cuponera.Any(x => x.IdIndicador == (int)eEstadoVigenciaCupon.Vigente))
                                currentHija.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vencido);
                            else
                                currentHija.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);

                            var ultimoCuponHijo = instrumentoRentaFijaDTO.Cuponera.OrderByDescending(x => Helper.ConvertFechaStringToIdFecha(x.FechaCorte)).FirstOrDefault();
                            if (ultimoCuponHijo != null)
                                persistedRentaFijaHija.IdFechaVencimiento = Helper.ConvertFechaStringToIdFecha(ultimoCuponHijo.FechaCorte);

                            var cuponesVencidosHijo = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(persistedRentaFijaHija.IdRentaFija) &&
                                                                                                           irfc.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido).ToList();

                            persistedRentaFijaHija.ValorNominalVigente = persistedRentaFijaHija.ValorNominalInicial;
                            if (cuponesVencidosHijo.Any())
                            {
                                var amortisacion = cuponesVencidosHijo.Sum(x => x.ImporteAmortizacion);
                                persistedRentaFijaHija.ValorNominalVigente = persistedRentaFijaHija.ValorNominalInicial - amortisacion;
                            }

                            /* var nacionalidad = iIndicadorRepository.Get(instrumentoRentaFijaDTO.Gdn.IndNacionalidadGdn);
                             var tipoinstrumento = iTipoInstrumentoRepository.FirstOrDefault(x => x.CodigoSbsTipoInstrumento == nacionalidad.ValorAuxChar1);
                             if (tipoinstrumento != null)*/
                            persistedRentaFijaHija.Instrumento.IdTipoInstrumento = instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento;
                        }
                    }
                    iIndicadorRepository.UnitOfWork.Commit();
                    transactionScope.Complete();
                }
            }
            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public InstrumentoRentaFijaDTO GetByIdInstrumentoRentaFija(int idInstrumento, int idRentaFija)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.RentaFija = iInstrumentoRentaFijaRepository.Get(idRentaFija);

            if (instrumento.RentaFija == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string FechaEmision = Helper.ConvertIdFechaToFechaString(instrumento.RentaFija.IdFechaEmision);
            string FechaVencimiento = Helper.ConvertIdFechaToFechaString(instrumento.RentaFija.IdFechaVencimiento);
            string FechaMontoColocado = Helper.ConvertIdFechaToFechaString(instrumento.RentaFija.IdFechaMontoColocado);
            string FechaIndicadorInteres = instrumento.RentaFija.IdFechaIndicadorInteres == null || instrumento.RentaFija.IdFechaIndicadorInteres == 0 ? string.Empty : Helper.ConvertIdFechaToFechaString(instrumento.RentaFija.IdFechaIndicadorInteres.Value);
            string FechaFinPeriodoGracia = instrumento.RentaFija.IdFechaFinPeriodoGracia == null || instrumento.RentaFija.IdFechaFinPeriodoGracia == 0 ? string.Empty : Helper.ConvertIdFechaToFechaString(instrumento.RentaFija.IdFechaFinPeriodoGracia.Value);

            instrumento.RentaFija.SecuencialFechaEmision = FechaEmision;
            instrumento.RentaFija.SecuencialFechaVencimiento = FechaVencimiento;
            instrumento.RentaFija.SecuencialFechaMontoColocado = FechaMontoColocado;
            instrumento.RentaFija.SecuencialFechaIndicadorInteres = FechaIndicadorInteres;
            instrumento.RentaFija.SecuencialFechaFinPeriodoGracia = FechaFinPeriodoGracia;

            instrumento.RentaFija.RentaFijaCupon = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumento.RentaFija.IdRentaFija)).ToList();

            InstrumentoRentaFijaDTO instrumentoRentaFijaDTO = instrumento.ProjectedAs<InstrumentoRentaFijaDTO>();

            instrumentoRentaFijaDTO.Cuponera = instrumento.RentaFija.RentaFijaCupon.ProjectedAsCollection<InstrumentoCuponeraListadoDTO>().ToArray();
            List<IndicadorDTO> indicadoresEstadoVigenciaCupon = iIndicadorAppService.GetAllByTipoIndicadorAndActive((int)eIndicador.EstadoVigenciaCupon);
            List<IndicadorDTO> indicadoresEstadoCupon = iIndicadorAppService.GetAllByTipoIndicadorAndActive((int)eIndicador.EstadoCupon);

            var tieneVencimientosEnFMS = false;
            for (int i = 0; i < instrumentoRentaFijaDTO.Cuponera.Length; i++)
            {
                IndicadorDTO indicadorVigenciaCupon = indicadoresEstadoVigenciaCupon.Find(ic => ic.Id.Equals(instrumentoRentaFijaDTO.Cuponera[i].IdIndicador));
                IndicadorDTO indicadorCupon = indicadoresEstadoCupon.Find(ic => ic.Id.Equals(instrumentoRentaFijaDTO.Cuponera[i].IdIndicadorEstadoCupon));
                instrumentoRentaFijaDTO.Cuponera[i].FechaInicio = instrumentoRentaFijaDTO.Cuponera[i].FechaInicioDate.ToString("dd/MM/yyyy");
                instrumentoRentaFijaDTO.Cuponera[i].FechaCorte = instrumentoRentaFijaDTO.Cuponera[i].FechaCorteDate.ToString("dd/MM/yyyy");
                instrumentoRentaFijaDTO.Cuponera[i].FechaPago = instrumentoRentaFijaDTO.Cuponera[i].FechaPagoDate.ToString("dd/MM/yyyy");
                instrumentoRentaFijaDTO.Cuponera[i].Indicador = indicadorVigenciaCupon.Descripcion;
                instrumentoRentaFijaDTO.Cuponera[i].IdIndicador = indicadorVigenciaCupon.IdIndicador;
                instrumentoRentaFijaDTO.Cuponera[i].EstadoCupon = indicadorCupon.Descripcion;
                instrumentoRentaFijaDTO.Cuponera[i].IdIndicadorEstadoCupon = indicadorCupon.IdIndicador;

                var cupon = instrumento.RentaFija.RentaFijaCupon.FirstOrDefault(x => x.NumeroCupon == instrumentoRentaFijaDTO.Cuponera[i].NumeroCupon);
                if (cupon != null && indicadorVigenciaCupon.IdIndicador == (int)eEstadoVigenciaCupon.Vencido)
                {
                    if (!tieneVencimientosEnFMS && cupon.VencimientoRentaFijaCupon.Any(x => x.EstadoVencimiento.IdIndicador == (int)eEstadoVigenciaCupon.Vencido))
                        tieneVencimientosEnFMS = true;
                }
            }
            InstrumentoRentaFija rentaFija = null;


            InstrumentoRentaFijaGdnDTO rentaFijaHija;

            if (instrumento.RentaFija.IsGdn == true)
            {
                rentaFija = iInstrumentoRentaFijaRepository.Get(instrumento.RentaFija.IdRentaFijaPrincipal.Value);

                instrumentoRentaFijaDTO.Cuponera = rentaFija.RentaFijaCupon.ProjectedAsCollection<InstrumentoCuponeraListadoDTO>().ToArray();
                for (int i = 0; i < instrumentoRentaFijaDTO.Cuponera.Length; i++)
                {
                    IndicadorDTO indicadorVigenciaCupon = indicadoresEstadoVigenciaCupon.Find(ic => ic.Id.Equals(instrumentoRentaFijaDTO.Cuponera[i].IdIndicador));
                    IndicadorDTO indicadorCupon = indicadoresEstadoCupon.Find(ic => ic.Id.Equals(instrumentoRentaFijaDTO.Cuponera[i].IdIndicadorEstadoCupon));
                    instrumentoRentaFijaDTO.Cuponera[i].FechaInicio = instrumentoRentaFijaDTO.Cuponera[i].FechaInicioDate.ToString("dd/MM/yyyy");
                    instrumentoRentaFijaDTO.Cuponera[i].FechaCorte = instrumentoRentaFijaDTO.Cuponera[i].FechaCorteDate.ToString("dd/MM/yyyy");
                    instrumentoRentaFijaDTO.Cuponera[i].FechaPago = instrumentoRentaFijaDTO.Cuponera[i].FechaPagoDate.ToString("dd/MM/yyyy");
                    instrumentoRentaFijaDTO.Cuponera[i].Indicador = indicadorVigenciaCupon.Descripcion;
                    instrumentoRentaFijaDTO.Cuponera[i].IdIndicador = indicadorVigenciaCupon.IdIndicador;
                    instrumentoRentaFijaDTO.Cuponera[i].EstadoCupon = indicadorCupon.Descripcion;
                    instrumentoRentaFijaDTO.Cuponera[i].IdIndicadorEstadoCupon = indicadorCupon.IdIndicador;
                }

            }
            else
            {
                rentaFija = iInstrumentoRentaFijaRepository.FirstOrDefault(x => x.IdRentaFijaPrincipal == idRentaFija);
            }

            if (rentaFija != null)
            {
                if (instrumento.RentaFija.IsGdn == true)
                {
                    string fecha = Helper.ConvertIdFechaToFechaString(instrumento.RentaFija.IdSecuencialFechaGdn);
                    rentaFijaHija = new InstrumentoRentaFijaGdnDTO();
                    rentaFijaHija.IdRentaFijaGdn = instrumento.RentaFija.IdRentaFija;
                    rentaFijaHija.IdInstrumentoGdn = instrumento.RentaFija.IdInstrumento;
                    rentaFijaHija.Nombre = instrumento.NombreInstrumento;
                    rentaFijaHija.IdTipoInstrumento = instrumento.IdTipoInstrumento.Value;
                    rentaFijaHija.Nemotecnico = instrumento.RentaFija.Nemotecnico;
                    rentaFijaHija.ValorNominal = instrumento.RentaFija.ValorNominalInicial;
                    rentaFijaHija.IdMoneda = instrumento.IdMoneda;
                    rentaFijaHija.CodIsin = instrumento.RentaFija.CodigoIsin;
                    rentaFijaHija.MontoEmitido = instrumento.RentaFija.MontoEmitido;
                    rentaFijaHija.CodigoSbs = instrumento.CodigoSbs;
                    rentaFijaHija.Fecha = fecha;
                    rentaFijaHija.IdSecuencialFecha = instrumento.RentaFija.IdSecuencialFechaGdn == null ? 0 : (int)instrumento.RentaFija.IdSecuencialFechaGdn;
                    rentaFijaHija.FactorConversion = instrumento.RentaFija.FactorConversion == null ? 0 : (decimal)instrumento.RentaFija.FactorConversion;
                    rentaFijaHija.ComentarioAnulacion = instrumento.ComentarioAnulacion;
                    rentaFijaHija.IndHabilitacionRiesgo = instrumento.IndHabilitacionRiesgo;
                    rentaFijaHija.ComentarioHabilitacion = instrumento.ComentarioHabilitacion;
                    instrumentoRentaFijaDTO.Gdn = rentaFijaHija;
                }
                else
                {
                    Instrumento instrumentoHija = iInstrumentoRepository.Get(rentaFija.IdInstrumento);
                    string fecha = Helper.ConvertIdFechaToFechaString(rentaFija.IdSecuencialFechaGdn);
                    rentaFijaHija = new InstrumentoRentaFijaGdnDTO();
                    rentaFijaHija.IdRentaFijaGdn = rentaFija.IdRentaFija;
                    rentaFijaHija.IdInstrumentoGdn = rentaFija.IdInstrumento;
                    rentaFijaHija.Nombre = instrumentoHija.NombreInstrumento;
                    rentaFijaHija.IdTipoInstrumento = instrumentoHija.IdTipoInstrumento.Value;
                    rentaFijaHija.Nemotecnico = rentaFija.Nemotecnico;
                    rentaFijaHija.ValorNominal = rentaFija.ValorNominalInicial;
                    rentaFijaHija.IdMoneda = instrumentoHija.IdMoneda;
                    rentaFijaHija.CodIsin = rentaFija.CodigoIsin;
                    rentaFijaHija.MontoEmitido = rentaFija.MontoEmitido;
                    rentaFijaHija.CodigoSbs = instrumentoHija.CodigoSbs;
                    rentaFijaHija.Fecha = fecha;
                    rentaFijaHija.IdSecuencialFecha = rentaFija.IdSecuencialFechaGdn == null ? 0 : (int)rentaFija.IdSecuencialFechaGdn;
                    rentaFijaHija.FactorConversion = rentaFija.FactorConversion == null ? 0 : (decimal)rentaFija.FactorConversion;
                    rentaFijaHija.ComentarioAnulacion = instrumentoHija.ComentarioAnulacion;
                    rentaFijaHija.IndHabilitacionRiesgo = instrumentoHija.IndHabilitacionRiesgo;
                    rentaFijaHija.ComentarioHabilitacion = instrumentoHija.ComentarioHabilitacion;
                    instrumentoRentaFijaDTO.Gdn = rentaFijaHija;
                }

            }
            instrumentoRentaFijaDTO.Cuponera = instrumentoRentaFijaDTO.Cuponera.OrderBy(x => x.NumeroCupon).ToArray();
            instrumentoRentaFijaDTO.TieneVencimientosEnFMS = tieneVencimientosEnFMS;
            return instrumentoRentaFijaDTO;
        }

        public string AddNewInstrumentoRentaFija(InstrumentoRentaFijaDTO instrumentoRentaFijaDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoRentaFijaDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoRentaFijaDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                VerifyNombreInstrumentoIsUnique(instrumentoRentaFijaDTO.CodigoSbs, instrumentoRentaFijaDTO.IdInstrumento);
                VerifyInstrumentoRentaFijaIsUnique(instrumentoRentaFijaDTO);
                int IdFechaTest = Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaEmision);
                int IdFechaEmision = (instrumentoRentaFijaDTO.SecuencialFechaEmision.Equals("_")) ? 0 : IdFechaTest;
                int IdFechaVencimiento = (instrumentoRentaFijaDTO.SecuencialFechaVencimiento.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaVencimiento);
                int IdFechaMontoColocado = (instrumentoRentaFijaDTO.SecuencialFechaMontoColocado.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaMontoColocado);
                int? IdFechaIndicadorInteres = (instrumentoRentaFijaDTO.SecuencialFechaIndicadorInteres.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaIndicadorInteres);
                int? IdFechaFinPeriodoGracia = (instrumentoRentaFijaDTO.SecuencialFechaFinPeriodoGracia.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.SecuencialFechaFinPeriodoGracia);
                int indEstadoVigenciaCuponVencido = iIndicadorAppService.GetId((int)eIndicador.EstadoVigenciaCupon, (int)eEstadoVigenciaCupon.Vencido);

                MonedaDTO monedaIndexada = VerifyMonedaIndexada(instrumentoRentaFijaDTO.IndTipoInteres, instrumentoRentaFijaDTO.IdMoneda, true);
                if (monedaIndexada.IdMoneda > 0)
                {
                    StringBuilder codigoSBS = new StringBuilder(instrumentoRentaFijaDTO.CodigoSbs);
                    codigoSBS[6] = monedaIndexada.CodigoSBS[0];
                    instrumentoRentaFijaDTO.CodigoSbs = codigoSBS.ToString();
                }

                string codigoSbsGenerated = "";
                if (instrumentoRentaFijaDTO.Gdn != null)
                {
                    codigoSbsGenerated = GeneratedCodigoSbsOnlyFirst7Digits(instrumentoRentaFijaDTO.Gdn.CodigoSbs, instrumentoRentaFijaDTO.Gdn.IdMoneda,
                                                                            instrumentoRentaFijaDTO.IdEmisor, instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento, 2);
                    MonedaDTO monedaIndexadaGdn = VerifyMonedaIndexada(instrumentoRentaFijaDTO.IndTipoInteres, instrumentoRentaFijaDTO.Gdn.IdMoneda, true);
                    if (monedaIndexadaGdn.IdMoneda > 0)
                    {
                        StringBuilder codigoSBS = new StringBuilder(codigoSbsGenerated);
                        codigoSBS[6] = monedaIndexadaGdn.CodigoSBS[0];
                        codigoSbsGenerated = codigoSBS.ToString();
                    }
                }
                //IndicadorDTO indicadorInteres = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.IndicadorInteresesBonos, instrumentoRentaFijaDTO.IndTipoInteres);
                //if (indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon))
                //    instrumentoRentaFijaDTO.Cuponera = GenerarCeroCupon(instrumentoRentaFijaDTO.SecuencialFechaEmision, instrumentoRentaFijaDTO.SecuencialFechaVencimiento, instrumentoRentaFijaDTO.ValorNominalInicial, instrumentoRentaFijaDTO.IdMercado);


                /*Instrumento instrumento = new Instrumento(instrumentoRentaFijaDTO.IdTipoInstrumento, instrumentoRentaFijaDTO.NombreInstrumento, instrumentoRentaFijaDTO.CodigoSbs,
                    instrumentoRentaFijaDTO.IdMoneda, instrumentoRentaFijaDTO.IdEmisor, IdGrupoInstrumento, instrumentoRentaFijaDTO.IndCategoria,
                    instrumentoRentaFijaDTO.IndFamilia, indActividad, instrumentoRentaFijaDTO.IdClasificacionRiesgo, instrumentoRentaFijaDTO.LoginActualizacion,
                    indHabilitacionRiesgo);*/

                Instrumento instrumento = new Instrumento(instrumentoRentaFijaDTO.IdTipoInstrumento, instrumentoRentaFijaDTO.NombreInstrumento, instrumentoRentaFijaDTO.CodigoSbs,
                    instrumentoRentaFijaDTO.IdMoneda, instrumentoRentaFijaDTO.IdEmisor, IdGrupoInstrumento, instrumentoRentaFijaDTO.IndCategoria,
                    instrumentoRentaFijaDTO.IndFamilia, indActividad, instrumentoRentaFijaDTO.IdClasificacionRiesgo, instrumentoRentaFijaDTO.LoginActualizacion,
                    indHabilitacionRiesgo, instrumentoRentaFijaDTO.IdTipoInteres, instrumentoRentaFijaDTO.IdTipoCalculoInteres);

                SaveInstrumento(instrumento);

                InstrumentoRentaFija instrumentoRentaFija = new InstrumentoRentaFija(instrumento.IdInstrumento, instrumentoRentaFijaDTO.Nemotecnico,
                    instrumentoRentaFijaDTO.CodigoIsin, instrumentoRentaFijaDTO.TieneMonedaDual, instrumentoRentaFijaDTO.IdMonedaDual,
                    IdFechaEmision, IdFechaVencimiento, IdFechaMontoColocado, IdFechaIndicadorInteres, instrumentoRentaFijaDTO.ValorNominalInicial,
                    instrumentoRentaFijaDTO.ValorNominalVigente, instrumentoRentaFijaDTO.ValorNominalSbs, instrumentoRentaFijaDTO.IndTipoUnidadEmision,
                    instrumentoRentaFijaDTO.IndTipoCustodia, instrumentoRentaFijaDTO.MontoEmitido, instrumentoRentaFijaDTO.MontoColocado, instrumentoRentaFijaDTO.IndTipoInteres,
                    instrumentoRentaFijaDTO.TieneMandato, instrumentoRentaFijaDTO.IndClasificadora1, instrumentoRentaFijaDTO.IndRatingClasificacion1,
                    instrumentoRentaFijaDTO.IndClasificadora2, instrumentoRentaFijaDTO.IndRatingClasificacion2, instrumentoRentaFijaDTO.Emision,
                    instrumentoRentaFijaDTO.Programa, instrumentoRentaFijaDTO.Serie, instrumentoRentaFijaDTO.IndRegionEmision, instrumentoRentaFijaDTO.IndPaisEmision,
                    instrumentoRentaFijaDTO.IndFocoGeograficoEmision, instrumentoRentaFijaDTO.IndClase, instrumentoRentaFijaDTO.IndTipoAmortizacion,
                    instrumentoRentaFijaDTO.IndPeriodoPago, instrumentoRentaFijaDTO.IndBaseCalculo, instrumentoRentaFijaDTO.NroCupones,
                    instrumentoRentaFijaDTO.IndIndexarInflacion, instrumentoRentaFijaDTO.PorcentajeTasaFija, instrumentoRentaFijaDTO.IndTasaFlotante,
                    instrumentoRentaFijaDTO.PorcentajeTasaFlotante, instrumentoRentaFijaDTO.IndTipoPeriodoGracia, IdFechaFinPeriodoGracia,
                    instrumentoRentaFijaDTO.IndOpcionalidad, instrumentoRentaFijaDTO.Observaciones, instrumentoRentaFijaDTO.LoginActualizacion, instrumentoRentaFijaDTO.IdMercado, instrumentoRentaFijaDTO.IdTipoTasa, false);

                SaveInstrumentoRentaFija(instrumentoRentaFija);


                //Generar cuponera
                foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoRentaFijaDTO.Cuponera)
                {
                    AddNewRentaFijaCupon(cupon, instrumentoRentaFija.IdRentaFija, instrumentoRentaFijaDTO.LoginActualizacion);
                }
                if (!instrumentoRentaFijaDTO.Cuponera.Any(x => x.IdIndicador == (int)eEstadoVigenciaCupon.Vigente))
                    instrumento.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vencido);

                var ultimoCupon = instrumentoRentaFijaDTO.Cuponera.OrderByDescending(x => Helper.ConvertFechaStringToIdFecha(x.FechaCorte)).FirstOrDefault();
                if (ultimoCupon != null)
                    instrumentoRentaFija.IdFechaVencimiento = Helper.ConvertFechaStringToIdFecha(ultimoCupon.FechaCorte);

                var cuponesVencidosGuardados = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumentoRentaFija.IdRentaFija) &&
                                                                                                   irfc.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido).ToList();

                instrumentoRentaFija.ValorNominalVigente = instrumentoRentaFija.ValorNominalInicial;
                if (cuponesVencidosGuardados.Any())
                {
                    var amortisacion = cuponesVencidosGuardados.Sum(x => x.ImporteAmortizacion);
                    instrumentoRentaFija.ValorNominalVigente = instrumentoRentaFija.ValorNominalInicial - amortisacion;

                    var ultimoCuponVencido = cuponesVencidosGuardados.OrderByDescending(x => Helper.ConvertToIdFecha(x.FechaCorte)).FirstOrDefault();
                    instrumento.VariacionValorNominalVigente.Add(new VariacionValorNominalVigente
                    {
                        IdSecuencialFecha = Helper.ConvertToIdFecha(ultimoCuponVencido.FechaCorte),
                        ValorNominalVigente = instrumentoRentaFija.ValorNominalVigente
                    });
                }
                else
                {
                    var primerCuponVigente = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumentoRentaFija.IdRentaFija) &&
                                                                                                      irfc.IndEstadoVigenciaCupon != indEstadoVigenciaCuponVencido)
                                                                                 .OrderBy(x => x.FechaCorte)
                                                                                 .FirstOrDefault();
                    instrumento.VariacionValorNominalVigente.Add(new VariacionValorNominalVigente
                    {
                        IdSecuencialFecha = Helper.ConvertToIdFecha(primerCuponVigente.FechaInicio),
                        ValorNominalVigente = instrumentoRentaFija.ValorNominalVigente
                    });
                }

                if (instrumentoRentaFijaDTO.Gdn != null)
                {
                    Instrumento instrumentoHija = new Instrumento(instrumentoRentaFijaDTO.IdTipoInstrumento, instrumentoRentaFijaDTO.Gdn.Nombre,
                    codigoSbsGenerated, instrumentoRentaFijaDTO.Gdn.IdMoneda, instrumentoRentaFijaDTO.IdEmisor, IdGrupoInstrumento,
                    instrumentoRentaFijaDTO.IndCategoria, instrumentoRentaFijaDTO.IndFamilia, indActividad, instrumentoRentaFijaDTO.IdClasificacionRiesgo,
                    instrumentoRentaFijaDTO.LoginActualizacion, indHabilitacionRiesgo);

                    SaveInstrumento(instrumentoHija);

                    int idFecha = Helper.ConvertFechaStringToIdFecha(instrumentoRentaFijaDTO.Gdn.Fecha);

                    InstrumentoRentaFija instrumentoRentaFijaHija = new InstrumentoRentaFija(instrumentoRentaFija, instrumentoHija.IdInstrumento,
                            instrumentoRentaFijaDTO.Gdn.Nombre, instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento, instrumentoRentaFijaDTO.Gdn.Nemotecnico,
                            instrumentoRentaFijaDTO.Gdn.ValorNominal, instrumentoRentaFijaDTO.Gdn.IdMoneda, instrumentoRentaFijaDTO.Gdn.CodIsin,
                            instrumentoRentaFijaDTO.Gdn.MontoEmitido, codigoSbsGenerated, idFecha,
                            instrumentoRentaFijaDTO.Gdn.FactorConversion, instrumentoRentaFija.IdRentaFija, true);

                    SaveInstrumentoRentaFija(instrumentoRentaFijaHija);

                    // Generar cuponera
                    foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoRentaFijaDTO.Cuponera)
                    {
                        AddNewRentaFijaCupon(cupon, instrumentoRentaFijaHija.IdRentaFija, instrumentoRentaFijaHija.LoginActualizacion);
                    }

                    if (!instrumentoRentaFijaDTO.Cuponera.Any(x => x.IdIndicador == (int)eEstadoVigenciaCupon.Vigente))
                        instrumentoHija.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vencido);

                    var ultimoCuponHijo = instrumentoRentaFijaDTO.Cuponera.OrderByDescending(x => Helper.ConvertFechaStringToIdFecha(x.FechaCorte)).FirstOrDefault();
                    if (ultimoCuponHijo != null)
                        instrumentoRentaFijaHija.IdFechaVencimiento = Helper.ConvertFechaStringToIdFecha(ultimoCuponHijo.FechaCorte);


                    var cuponesVencidosGuardadosHijo = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumentoRentaFijaHija.IdRentaFija) &&
                                                                                                  irfc.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido).ToList();

                    instrumentoRentaFijaHija.ValorNominalVigente = instrumentoRentaFijaHija.ValorNominalInicial;
                    if (cuponesVencidosGuardados.Any())
                    {
                        var amortisacion = cuponesVencidosGuardadosHijo.Sum(x => x.ImporteAmortizacion);
                        instrumentoRentaFijaHija.ValorNominalVigente = instrumentoRentaFijaHija.ValorNominalInicial - amortisacion;
                    }/*
                    var nacionalidad = iIndicadorRepository.Get(instrumentoRentaFijaDTO.Gdn.IndNacionalidadGdn);
                    var tipoinstrumento = iTipoInstrumentoRepository.FirstOrDefault(x => x.CodigoSbsTipoInstrumento == nacionalidad.ValorAuxChar1);
                    if (tipoinstrumento != null)*/
                    instrumentoHija.IdTipoInstrumento = instrumentoRentaFijaDTO.Gdn.IdTipoInstrumento;
                }
                iIndicadorRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_RegistrarElemento;
        }
        public string RemoveInstrumentoRentaFija(int idInstrumento, int idRentaFija)
        {
            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);

            InstrumentoRentaFija persistedRentaFija = iInstrumentoRentaFijaRepository.Get(idRentaFija);

            List<InstrumentoRentaFijaCupon> persistedRentaFijaCupon = iInstrumentoRentaFijaCuponRepository.GetFiltered(obj => obj.IdRentaFija.Equals(idRentaFija)).ToList();

            if (persisted == null || persistedRentaFija == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            if (persisted == null || persistedRentaFija == null || persistedRentaFijaCupon.Count() <= 0)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            foreach (InstrumentoRentaFijaCupon instrumentoRentaFijaCupon in persistedRentaFijaCupon)
            {
                iInstrumentoRentaFijaCuponRepository.Remove(instrumentoRentaFijaCupon);
            }

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_RentaFija_Tiene_Dependencias));

            //

            InstrumentoRentaFija persistedRentaFijaGdn = iInstrumentoRentaFijaRepository.GetFiltered(x => x.IdRentaFijaPrincipal == idRentaFija).FirstOrDefault();

            Instrumento persistedPadre = new Instrumento();
            InstrumentoRentaFija persistedRentaFijaHija = new InstrumentoRentaFija();
            List<InstrumentoRentaFijaCupon> persistedRentaFijaCuponHija = new List<InstrumentoRentaFijaCupon>();
            if (persistedRentaFijaGdn != null)
            {
                persistedPadre = iInstrumentoRepository.Get(persistedRentaFijaGdn.IdInstrumento);
                persistedRentaFijaHija = iInstrumentoRentaFijaRepository.GetFiltered(x => x.IdRentaFijaPrincipal == persistedRentaFijaGdn.IdRentaFija).FirstOrDefault();
                persistedRentaFijaCuponHija = iInstrumentoRentaFijaCuponRepository.GetFiltered(obj => obj.IdRentaFija.Equals(persistedRentaFijaGdn.IdRentaFija)).ToList();

                if (persistedRentaFijaHija != null)
                    iInstrumentoRentaFijaRepository.Remove(persistedRentaFijaHija);
                if (persistedPadre != null)
                    iInstrumentoRepository.RemoveInstrumentoOnCascade(persistedPadre);

                foreach (InstrumentoRentaFijaCupon instrumentoRentaFijaCupon in persistedRentaFijaCuponHija.ToList())
                {
                    iInstrumentoRentaFijaCuponRepository.Remove(instrumentoRentaFijaCupon);
                }
            }
            iInstrumentoRentaFijaRepository.Remove(persistedRentaFija);
            iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
            iInstrumentoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_EliminarElemento;
        }
        public string AnnulInstrumentoRentaFija(InstrumentoRentaFijaDTO instrumentoRentaFijaDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoRentaFijaDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoRentaFijaDTO.IdInstrumento);
                InstrumentoRentaFija persistedRentaFija = iInstrumentoRentaFijaRepository.Get(instrumentoRentaFijaDTO.IdRentaFija);
                // List<InstrumentoRentaFijaCupon> persistedRentaFijaCupon = iInstrumentoRentaFijaCuponRepository.GetFiltered(obj => obj.IdRentaFija.Equals(instrumentoRentaFijaDTO.IdRentaFija)).ToList();


                int idAnulado = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

                Instrumento persistedPadre = new Instrumento();
                InstrumentoRentaFija persistedRentaFijaHija = new InstrumentoRentaFija();
                string comment = "";
                if (instrumentoRentaFijaDTO.Gdn != null)
                {
                    persistedPadre = iInstrumentoRepository.Get(instrumentoRentaFijaDTO.Gdn.IdInstrumentoGdn);
                    persistedRentaFijaHija = iInstrumentoRentaFijaRepository.GetFiltered(x => x.IdRentaFija == instrumentoRentaFijaDTO.Gdn.IdRentaFijaGdn).FirstOrDefault();

                    comment = String.IsNullOrEmpty(instrumentoRentaFijaDTO.Gdn.ComentarioAnulacion) ? instrumentoRentaFijaDTO.ComentarioAnulacion : instrumentoRentaFijaDTO.Gdn.ComentarioAnulacion;

                    Instrumento currentHija = new Instrumento(comment, instrumentoRentaFijaDTO.LoginActualizacion);
                    if (persistedPadre != null)
                    {
                        currentHija.IdInstrumento = persistedPadre.IdInstrumento;

                        persistedPadre.IndActividad = idAnulado;
                        persistedPadre.ComentarioAnulacion = currentHija.ComentarioAnulacion;
                        persistedPadre.LoginActualizacion = currentHija.LoginActualizacion;
                        persistedPadre.FechaHoraActualizacion = currentHija.FechaHoraActualizacion;
                        persistedPadre.UsuarioActualizacion = currentHija.UsuarioActualizacion;

                        persistedRentaFijaHija.LoginActualizacion = currentHija.LoginActualizacion;
                        persistedRentaFijaHija.FechaHoraActualizacion = currentHija.FechaHoraActualizacion;
                        persistedRentaFijaHija.UsuarioActualizacion = currentHija.UsuarioActualizacion;

                        iInstrumentoRepository.Merge(persistedPadre, persistedPadre);
                        iInstrumentoRepository.UnitOfWork.Commit();
                        iInstrumentoRentaFijaRepository.Merge(persistedRentaFijaHija, persistedRentaFijaHija);
                        iInstrumentoRentaFijaRepository.UnitOfWork.Commit();
                    }
                }

                Instrumento current = new Instrumento(instrumentoRentaFijaDTO.ComentarioAnulacion,
                     instrumentoRentaFijaDTO.LoginActualizacion);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndActividad = idAnulado;
                persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedRentaFija.LoginActualizacion = current.LoginActualizacion;
                persistedRentaFija.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedRentaFija.UsuarioActualizacion = current.UsuarioActualizacion;

                //foreach (InstrumentoRentaFijaCupon instrumentoRentaFijaCupon in persistedRentaFijaCupon)
                //{
                //    instrumentoRentaFijaCupon.LoginActualizacion = current.LoginActualizacion;
                //    instrumentoRentaFijaCupon.FechaHoraActualizacion = current.FechaHoraActualizacion;
                //    instrumentoRentaFijaCupon.UsuarioActualizacion = current.UsuarioActualizacion;
                //    iInstrumentoRentaFijaCuponRepository.Merge(instrumentoRentaFijaCupon, instrumentoRentaFijaCupon);
                //    iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();
                //}                               

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoRentaFijaRepository.Merge(persistedRentaFija, persistedRentaFija);
                iInstrumentoRentaFijaRepository.UnitOfWork.Commit();

                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_AnularElemento;
        }

        public string ActiveInstrumentoRentaFija(InstrumentoRentaFijaDTO instrumentoRentaFijaDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoRentaFijaDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);
                Instrumento persisted = iInstrumentoRepository.Get(instrumentoRentaFijaDTO.IdInstrumento);
                if (!iAnexoIRubroRepository.Any(p => p.AnexoIRubroDetalleInstrumento.Any(q => q.IdTipoInstrumento == persisted.TipoInstrumento.IdTipoInstrumento && q.IdEmisor == persisted.IdEmisor)))
                    throw new ApplicationException("No tiene detalle en el Anexo III");

                InstrumentoRentaFija persistedRentaFija = iInstrumentoRentaFijaRepository.Get(instrumentoRentaFijaDTO.IdRentaFija);
                // List<InstrumentoRentaFijaCupon> persistedRentaFijaCupon = iInstrumentoRentaFijaCuponRepository.GetFiltered(obj => obj.IdRentaFija.Equals(instrumentoRentaFijaDTO.IdRentaFija)).ToList();


                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoRentaFijaDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoRentaFijaDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;
                Instrumento current = new Instrumento(instrumentoRentaFijaDTO.ComentarioHabilitacion,
                    instrumentoRentaFijaDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedRentaFija.LoginActualizacion = current.LoginActualizacion;
                persistedRentaFija.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedRentaFija.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoRentaFijaRepository.Merge(persistedRentaFija, persistedRentaFija);
                iInstrumentoRentaFijaRepository.UnitOfWork.Commit();

                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensaje;
        }

        public InteresDevengadoListadoDTO CalcularInteresesDevengados(int idInstrumento, DateTime? fechaInicio, DateTime fechaFin, int idFondo, decimal? cantidad = null)
        {
            InteresDevengadoListadoDTO interesDevengadoDTO = new InteresDevengadoListadoDTO();
            IdiDTO idiDTO = iIdiAppService.GetLastIDIClose(idFondo);
            int idSecuencialFechaFin = fechaFin.ConvertToIdFecha();

            var saldoContable = cantidad;
            if (saldoContable == null)
            {
                int idSecuencialFechaConsultaStock = idiDTO.IdSecuencialFechaIDI > idSecuencialFechaFin ? idSecuencialFechaFin : idiDTO.IdSecuencialFechaIDI;
                var stockInstrumento = iInstrumentoStockRepository.GetFiltered(ist => ist.IdInstrumento == idInstrumento &&
                                                                               ist.IdSecuencialFecha == idSecuencialFechaConsultaStock && ist.IdFondoPension == idFondo).ToArray();
                saldoContable = !stockInstrumento.Any() ? 0 : stockInstrumento.Sum(z => z.SaldoContable);
            }
            interesDevengadoDTO.CantidadContable = Convert.ToInt32(saldoContable.Value);
            interesDevengadoDTO.IdInstrumento = idInstrumento;
            int diasDevengados = 0;
            //Validamos que la fecha de inicio no sea menor a la fecha de fin
            if ((!fechaInicio.HasValue ? DateTime.MinValue : fechaInicio.Value) > fechaFin)
                throw new ArgumentException("La fecha de inicio no puede ser mayor a la fecha de fin para la consulta de calculo de intereses devengados.");

            //InstrumentoRentaFija instrumentoRentaFija = iInstrumentoRentaFijaRepository.FirstOrDefault(irf => irf.IdInstrumento == idInstrumento);
            if (iInstrumentoRentaFijaRepository.Any(irf => irf.IdInstrumento == idInstrumento))//instrumentoRentaFija != null)
            {
                InstrumentoRentaFija instrumentoRentaFija = iInstrumentoRentaFijaRepository.FirstOrDefault(irf => irf.IdInstrumento == idInstrumento);
                DateTime fechaVencimiento = Helper.ConvertIdFechaToDateTime(instrumentoRentaFija.IdFechaVencimiento);
                IndicadorDTO periodoVigenciaCupon = iIndicadorAppService.GetById(instrumentoRentaFija.IndPeriodoPago);
                //decimal cantidadDiasPeriodoPago = Convert.ToDecimal(periodoVigenciaCupon.ValorAuxNum1.Value);

                List<InstrumentoRentaFijaCupon> cuponesRentaFija = iInstrumentoRentaFijaCuponRepository.GetFiltered(irfc => irfc.IdRentaFija.Equals(instrumentoRentaFija.IdRentaFija)).ToList();
                if (cuponesRentaFija.Count == 0)
                    return interesDevengadoDTO;

                InstrumentoRentaFijaCupon cuponInicio = new InstrumentoRentaFijaCupon();
                if (fechaInicio.HasValue)
                {
                    cuponInicio = cuponesRentaFija.Find(crf => fechaInicio.Value >= crf.FechaInicio && fechaInicio.Value <= crf.FechaCorte);
                    if (cuponInicio == null)
                    {
                        InstrumentoRentaFijaCupon primerCupon = cuponesRentaFija.OrderBy(x => x.NumeroCupon).First();
                        if (fechaInicio < primerCupon.FechaInicio)
                            cuponInicio = primerCupon;
                    }
                }

                InstrumentoRentaFijaCupon cuponFin = cuponesRentaFija.Find(crf => fechaFin >= crf.FechaInicio && fechaFin <= crf.FechaCorte);

                //Calculamos los intereses devengados
                if (!fechaInicio.HasValue && cuponFin != null)
                {
                    decimal diasTranscurridos = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponFin.FechaInicio, fechaFin, fechaVencimiento, out diasDevengados);
                    decimal diasPeriodoVigenciaCupon = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponFin.FechaInicio, cuponFin.FechaCorte, fechaVencimiento, out diasDevengados);
                    if (diasPeriodoVigenciaCupon > 0)
                        interesDevengadoDTO.InteresesDevengados = Math.Round(cuponFin.ImporteInteres * /*interesDevengadoDTO.CantidadContable **/ diasTranscurridos / diasPeriodoVigenciaCupon, 7);
                    else
                        interesDevengadoDTO.InteresesDevengados = decimal.Zero;
                    interesDevengadoDTO.MontoAmortizacion = cuponFin.ImporteAmortizacion;
                    interesDevengadoDTO.MontoCupon = cuponFin.ImporteCupon;
                    interesDevengadoDTO.DiasDevengados = diasDevengados;
                }
                else if (cuponInicio != null && cuponFin != null)
                {
                    if (cuponInicio.NumeroCupon != cuponFin.NumeroCupon)
                    {
                        int numeroCuponActual = cuponInicio.NumeroCupon;
                        while (numeroCuponActual <= cuponFin.NumeroCupon)
                        {
                            decimal diasTranscurridos = decimal.Zero;
                            decimal diasPeriodoVigenciaCupon = decimal.Zero;
                            if (numeroCuponActual.Equals(cuponInicio.NumeroCupon))
                            {
                                diasTranscurridos = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponInicio.FechaInicio, fechaInicio.Value, fechaVencimiento, out diasDevengados);
                                diasPeriodoVigenciaCupon = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponInicio.FechaInicio, cuponInicio.FechaCorte, fechaVencimiento, out diasDevengados);
                                decimal interesesDevengados = decimal.Zero;
                                if (diasPeriodoVigenciaCupon > 0)
                                    interesesDevengados = Math.Round(cuponInicio.ImporteInteres /** interesDevengadoDTO.CantidadContable */* diasTranscurridos / diasPeriodoVigenciaCupon, 7);
                                interesDevengadoDTO.InteresesDevengados = cuponInicio.ImporteInteres - interesesDevengados;
                                interesDevengadoDTO.MontoAmortizacion = cuponInicio.ImporteAmortizacion;
                                interesDevengadoDTO.MontoCupon = cuponInicio.ImporteCupon;
                                interesDevengadoDTO.DiasDevengados = diasDevengados;
                            }
                            else if (numeroCuponActual > cuponInicio.NumeroCupon && numeroCuponActual < cuponFin.NumeroCupon)
                            {
                                interesDevengadoDTO.InteresesDevengados += cuponesRentaFija.Find(rfc => rfc.NumeroCupon.Equals(numeroCuponActual)).ImporteInteres;
                                interesDevengadoDTO.MontoAmortizacion += cuponesRentaFija.Find(rfc => rfc.NumeroCupon.Equals(numeroCuponActual)).ImporteAmortizacion;
                                interesDevengadoDTO.MontoCupon += cuponesRentaFija.Find(rfc => rfc.NumeroCupon.Equals(numeroCuponActual)).ImporteCupon;
                                interesDevengadoDTO.DiasDevengados += diasDevengados;
                            }
                            else if (numeroCuponActual.Equals(cuponFin.NumeroCupon))
                            {
                                diasTranscurridos = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponFin.FechaInicio, fechaFin, fechaVencimiento, out diasDevengados);
                                diasPeriodoVigenciaCupon = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponFin.FechaInicio, cuponFin.FechaCorte, fechaVencimiento, out diasDevengados);
                                decimal interesesDevengados = decimal.Zero;
                                if (diasPeriodoVigenciaCupon > 0)
                                    interesesDevengados = Math.Round(cuponFin.ImporteInteres * /*interesDevengadoDTO.CantidadContable **/ diasTranscurridos / diasPeriodoVigenciaCupon, 7);
                                interesDevengadoDTO.InteresesDevengados += interesesDevengados;
                                //Se establecen las demas propiedades
                                interesDevengadoDTO.MontoAmortizacion += cuponFin.ImporteAmortizacion;
                                interesDevengadoDTO.MontoCupon += cuponFin.ImporteCupon;
                                interesDevengadoDTO.DiasDevengados += diasDevengados;
                            }
                            //Pasamos al siguiente cupón
                            numeroCuponActual++;
                        }
                    }
                    else
                    {
                        decimal baseCalculoFechaInicio = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponInicio.FechaInicio, fechaInicio.Value, fechaVencimiento, out diasDevengados);
                        decimal baseCalculoFechaFin = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponInicio.FechaInicio, fechaFin, fechaVencimiento, out diasDevengados);
                        decimal diasPeriodoVigenciaCupon = ObtenerBaseCalculo(instrumentoRentaFija.IndBaseCalculo, cuponFin.FechaInicio, cuponFin.FechaCorte, fechaVencimiento, out diasDevengados);
                        if (diasPeriodoVigenciaCupon > 0)
                            interesDevengadoDTO.InteresesDevengados = Math.Round(cuponFin.ImporteInteres * /*interesDevengadoDTO.CantidadContable **/ (baseCalculoFechaFin - baseCalculoFechaInicio) / diasPeriodoVigenciaCupon, 7);
                        else
                            interesDevengadoDTO.InteresesDevengados = decimal.Zero;
                        interesDevengadoDTO.MontoAmortizacion = cuponFin.ImporteAmortizacion;
                        interesDevengadoDTO.MontoCupon = cuponFin.ImporteCupon;
                        interesDevengadoDTO.DiasDevengados = diasDevengados;
                    }
                }
            }
            else
            {
                InstrumentoCertificadoDepositoCortoPlazo instrumentoCertificadoDeposito = iInstrumentoCertificadoDepositoCortoPlazoRepository.GetFiltered(idp => idp.IdInstrumento.Equals(interesDevengadoDTO.IdInstrumento)).FirstOrDefault();
                List<InstrumentoCertificadoDepositoCortoPlazoCupon> cuponesCertificadoDeposito = iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.GetFiltered(icdc => icdc.IdCertificadoDepositoCortoPlazo.Equals(instrumentoCertificadoDeposito.IdCertificadoDepositoCortoPlazo)).ToList();


                DateTime fechaVencimiento = Helper.ConvertIdFechaToDateTime(instrumentoCertificadoDeposito.IdSecuencialFechaVencimiento);
                IndicadorDTO periodoVigenciaCupon = iIndicadorAppService.GetById(instrumentoCertificadoDeposito.IndPeriodoPago);
                //decimal cantidadDiasPeriodoPago = Convert.ToDecimal(periodoVigenciaCupon.ValorAuxNum1.Value);

                if (cuponesCertificadoDeposito.Count == 0)
                    return interesDevengadoDTO;

                InstrumentoCertificadoDepositoCortoPlazoCupon cuponInicio = new InstrumentoCertificadoDepositoCortoPlazoCupon();
                if (fechaInicio.HasValue)
                {
                    cuponInicio = cuponesCertificadoDeposito.Find(crf => fechaInicio.Value >= crf.FechaInicio && fechaInicio.Value <= crf.FechaCorte);
                    //if (cuponInicio == null)
                    //    throw new ArgumentException("No se encontró un cupón con un rango de fecha inicio y de corte que contenga la fecha de inicio.");
                }

                InstrumentoCertificadoDepositoCortoPlazoCupon cuponFin = cuponesCertificadoDeposito.Find(crf => fechaFin >= crf.FechaInicio && fechaFin <= crf.FechaCorte);
                //if (cuponFin == null)
                //    throw new ArgumentException("No se encontró un cupón con un rango de fecha inicio y de corte que contenga la fecha de fin o de consulta.");

                //Calculamos los intereses devengados
                if (!fechaInicio.HasValue && cuponFin != null)
                {
                    decimal diasTranscurridos = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponFin.FechaInicio, fechaFin, fechaVencimiento, out diasDevengados);
                    decimal diasPeriodoVigenciaCupon = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponFin.FechaInicio, cuponFin.FechaCorte, fechaVencimiento, out diasDevengados);
                    if (diasPeriodoVigenciaCupon > 0)
                        interesDevengadoDTO.InteresesDevengados = Math.Round(cuponFin.ImporteInteres /** interesDevengadoDTO.CantidadContable*/ * diasTranscurridos / diasPeriodoVigenciaCupon, 7);
                    else
                        interesDevengadoDTO.InteresesDevengados = decimal.Zero;
                    interesDevengadoDTO.MontoAmortizacion = cuponFin.ImporteAmortizacion;
                    interesDevengadoDTO.MontoCupon = cuponFin.ImporteCupon;
                    interesDevengadoDTO.DiasDevengados = diasDevengados;
                }
                else if (cuponInicio != null && cuponFin != null)
                {
                    if (cuponInicio.NumeroCupon != cuponFin.NumeroCupon)
                    {
                        int numeroCuponActual = cuponInicio.NumeroCupon;
                        while (numeroCuponActual <= cuponFin.NumeroCupon)
                        {
                            decimal diasTranscurridos = decimal.Zero;
                            decimal diasPeriodoVigenciaCupon = decimal.Zero;

                            if (numeroCuponActual.Equals(cuponInicio.NumeroCupon))
                            {
                                diasTranscurridos = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponInicio.FechaInicio, fechaInicio.Value, fechaVencimiento, out diasDevengados);
                                diasPeriodoVigenciaCupon = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponInicio.FechaInicio, cuponInicio.FechaCorte, fechaVencimiento, out diasDevengados);
                                decimal interesesDevengados = decimal.Zero;
                                if (diasPeriodoVigenciaCupon > 0)
                                    interesesDevengados = Math.Round(cuponInicio.ImporteInteres * /*interesDevengadoDTO.CantidadContable **/ diasTranscurridos / diasPeriodoVigenciaCupon, 7);
                                interesDevengadoDTO.InteresesDevengados = cuponInicio.ImporteInteres - interesesDevengados;
                                interesDevengadoDTO.MontoAmortizacion = cuponInicio.ImporteAmortizacion;
                                interesDevengadoDTO.MontoCupon = cuponInicio.ImporteCupon;
                                interesDevengadoDTO.DiasDevengados = diasDevengados;
                            }
                            else if (numeroCuponActual > cuponInicio.NumeroCupon && numeroCuponActual < cuponFin.NumeroCupon)
                            {
                                interesDevengadoDTO.InteresesDevengados += cuponesCertificadoDeposito.Find(rfc => rfc.NumeroCupon.Equals(numeroCuponActual)).ImporteInteres;
                                interesDevengadoDTO.MontoAmortizacion += cuponesCertificadoDeposito.Find(rfc => rfc.NumeroCupon.Equals(numeroCuponActual)).ImporteAmortizacion;
                                interesDevengadoDTO.MontoCupon += cuponesCertificadoDeposito.Find(rfc => rfc.NumeroCupon.Equals(numeroCuponActual)).ImporteCupon;
                                interesDevengadoDTO.DiasDevengados += diasDevengados;
                            }
                            else if (numeroCuponActual.Equals(cuponFin.NumeroCupon))
                            {
                                diasTranscurridos = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponFin.FechaInicio, fechaFin, fechaVencimiento, out diasDevengados);
                                diasPeriodoVigenciaCupon = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponFin.FechaInicio, cuponFin.FechaCorte, fechaVencimiento, out diasDevengados);
                                decimal interesesDevengados = decimal.Zero;
                                if (diasPeriodoVigenciaCupon > 0)
                                    interesesDevengados = Math.Round(cuponFin.ImporteInteres * /*interesDevengadoDTO.CantidadContable **/ diasTranscurridos / diasPeriodoVigenciaCupon, 7);
                                interesDevengadoDTO.InteresesDevengados += interesesDevengados;
                                //Se establecen las demas propiedades
                                interesDevengadoDTO.MontoAmortizacion += cuponFin.ImporteAmortizacion;
                                interesDevengadoDTO.MontoCupon += cuponFin.ImporteCupon;
                                interesDevengadoDTO.DiasDevengados += diasDevengados;
                            }
                            //Pasamos al siguiente cupón
                            numeroCuponActual++;
                        }
                    }
                    else
                    {
                        decimal baseCalculoFechaInicio = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponInicio.FechaInicio, fechaInicio.Value, fechaVencimiento, out diasDevengados);
                        decimal baseCalculoFechaFin = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponInicio.FechaInicio, fechaFin, fechaVencimiento, out diasDevengados);
                        decimal diasPeriodoVigenciaCupon = ObtenerBaseCalculo(instrumentoCertificadoDeposito.IndBaseCalculo, cuponFin.FechaInicio, cuponFin.FechaCorte, fechaVencimiento, out diasDevengados);
                        if (diasPeriodoVigenciaCupon > 0)
                            interesDevengadoDTO.InteresesDevengados = Math.Round(cuponFin.ImporteInteres * /*interesDevengadoDTO.CantidadContable **/ (baseCalculoFechaFin - baseCalculoFechaInicio) / diasPeriodoVigenciaCupon, 7);
                        else
                            interesDevengadoDTO.InteresesDevengados = decimal.Zero;
                        interesDevengadoDTO.MontoAmortizacion = cuponFin.ImporteAmortizacion;
                        interesDevengadoDTO.MontoCupon = cuponFin.ImporteCupon;
                        interesDevengadoDTO.DiasDevengados = diasDevengados;
                    }
                }
            }

            interesDevengadoDTO.InteresesDevengados = interesDevengadoDTO.InteresesDevengados * interesDevengadoDTO.CantidadContable;
            interesDevengadoDTO.MontoAmortizacion = interesDevengadoDTO.MontoAmortizacion * interesDevengadoDTO.CantidadContable;
            interesDevengadoDTO.MontoCupon = interesDevengadoDTO.MontoCupon * interesDevengadoDTO.CantidadContable;
            return interesDevengadoDTO;
        }
        #endregion

        #region InstrumentoNotasEstructuradasAppService Members

        public InstrumentoNotaEstructuradaPagedDTO GetFilteredDataNotasEstructuradas(string codigoSbs, int idEmisor, int idTipoSubyacente, string fechaEmision,
            string fechaVencimiento, int indActividad, int indHabilitacion, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;
            int idFechaEmision = (fechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fechaEmision);
            int idFechaVencimiento = (fechaVencimiento.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fechaVencimiento);
            InstrumentoNotaEstructuradaPagedDTO response = iInstrumentoDataRepository.GetFilteredDataNotasEstructuradas(codigoSbs, idEmisor, idTipoSubyacente, idFechaEmision, idFechaVencimiento, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);

            foreach (InstrumentoNotaEstructuradaListadoDTO item in response.ListaInstrumentoNotasEstructuradas)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }

            return response;
        }
        public string UpdateInstrumentoNotasEstructuradas(InstrumentoNotaEstructuradaDTO instrumentoNotasEstructuradasDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoNotasEstructuradasDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoNotasEstructuradasDTO"));

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoNotasEstructuradasDTO.IdInstrumento);
                InstrumentoNotaEstructurada persistedNotaEstructurada = iInstrumentoNotaEstructuradaRepository.Get(instrumentoNotasEstructuradasDTO.IdNotaEstructurada);

                if (persisted == null || persistedNotaEstructurada == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                VerifyNombreInstrumentoIsUnique(instrumentoNotasEstructuradasDTO.CodigoSbs, instrumentoNotasEstructuradasDTO.IdInstrumento);
                VerifyInstrumentoNotaEstructuradaIsUnique(instrumentoNotasEstructuradasDTO);

                int IdSecuencialFechaInicio = (instrumentoNotasEstructuradasDTO.SecuencialFechaInicio.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoNotasEstructuradasDTO.SecuencialFechaInicio);
                int IdSecuencialFechaVencimiento = (instrumentoNotasEstructuradasDTO.SecuencialFechaVencimiento.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoNotasEstructuradasDTO.SecuencialFechaVencimiento);
                int IdSecuencialFechaLiquidacion = (instrumentoNotasEstructuradasDTO.SecuencialFechaLiquidacion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoNotasEstructuradasDTO.SecuencialFechaLiquidacion);
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoNotasEstructuradasDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoNotasEstructuradasDTO.IndHabilitacionRiesgo);

                Instrumento current = new Instrumento(instrumentoNotasEstructuradasDTO.IdTipoInstrumento, instrumentoNotasEstructuradasDTO.NombreInstrumento, instrumentoNotasEstructuradasDTO.CodigoSbs,
                    instrumentoNotasEstructuradasDTO.IdMoneda, instrumentoNotasEstructuradasDTO.IdEmisor, IdGrupoInstrumento, instrumentoNotasEstructuradasDTO.IndCategoria,
                    instrumentoNotasEstructuradasDTO.IndFamilia, indActividad, instrumentoNotasEstructuradasDTO.IdClasificacionRiesgo, instrumentoNotasEstructuradasDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, current);
                iInstrumentoRepository.UnitOfWork.Commit();

                InstrumentoNotaEstructurada instrumentoNotasEstructuradas = new InstrumentoNotaEstructurada(
                    current.IdInstrumento, instrumentoNotasEstructuradasDTO.IndTipoSubyacente, instrumentoNotasEstructuradasDTO.IndSubyacente,
                    instrumentoNotasEstructuradasDTO.Nemotecnico, instrumentoNotasEstructuradasDTO.CodigoIsin, instrumentoNotasEstructuradasDTO.IndTipoUnidadColocacion,
                    instrumentoNotasEstructuradasDTO.TieneMonedaDual, instrumentoNotasEstructuradasDTO.IdMonedaDual,
                    instrumentoNotasEstructuradasDTO.MontoColocado, instrumentoNotasEstructuradasDTO.MontoEmitido,
                    instrumentoNotasEstructuradasDTO.ValorNominalInicial, instrumentoNotasEstructuradasDTO.ValorNominalSbs,
                    instrumentoNotasEstructuradasDTO.IndTipoCustodia, IdSecuencialFechaInicio,
                    IdSecuencialFechaVencimiento,
                    instrumentoNotasEstructuradasDTO.PlazoDias, IdSecuencialFechaLiquidacion,
                    instrumentoNotasEstructuradasDTO.PorcentajeCapitalGarantizado, instrumentoNotasEstructuradasDTO.IndValorizacion,
                    instrumentoNotasEstructuradasDTO.TieneMandato,
                    instrumentoNotasEstructuradasDTO.IndClase, instrumentoNotasEstructuradasDTO.Observaciones,
                    instrumentoNotasEstructuradasDTO.LoginActualizacion,
                    instrumentoNotasEstructuradasDTO.IndEmisorSubyacente,
                    instrumentoNotasEstructuradasDTO.IndContraparteSubyacente
                    );

                instrumentoNotasEstructuradas.IdNotaEstructurada = persistedNotaEstructurada.IdNotaEstructurada;
                iInstrumentoNotaEstructuradaRepository.Merge(persistedNotaEstructurada, instrumentoNotasEstructuradas);
                iInstrumentoNotaEstructuradaRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public InstrumentoNotaEstructuradaDTO GetByIdInstrumentoNotasEstructuradas(int idInstrumento, int idNotaEstructurada)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.NotaEstructurada = iInstrumentoNotaEstructuradaRepository.Get(idNotaEstructurada);

            if (instrumento.NotaEstructurada == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string SecuencialFechaInicio = Helper.ConvertIdFechaToFechaString(instrumento.NotaEstructurada.IdSecuencialFechaInicio);
            string SecuencialFechaLiquidacion = Helper.ConvertIdFechaToFechaString(instrumento.NotaEstructurada.IdSecuencialFechaLiquidacion);
            string SecuencialFechaVencimiento = Helper.ConvertIdFechaToFechaString(instrumento.NotaEstructurada.IdSecuencialFechaVencimiento);

            instrumento.NotaEstructurada.SecuencialFechaInicio = SecuencialFechaInicio;
            instrumento.NotaEstructurada.SecuencialFechaLiquidacion = SecuencialFechaLiquidacion;
            instrumento.NotaEstructurada.SecuencialFechaVencimiento = SecuencialFechaVencimiento;

            return instrumento.ProjectedAs<InstrumentoNotaEstructuradaDTO>();
        }
        public string AddNewInstrumentoNotasEstructuradas(InstrumentoNotaEstructuradaDTO instrumentoNotasEstructuradasDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoNotasEstructuradasDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoNotasEstructuradasDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);
                TipoInstrumentoDTO tipoInstrumentoDTO = iTipoInstrumentoAppService.GetById(instrumentoNotasEstructuradasDTO.IdTipoInstrumento.Value);
                VerifyNombreInstrumentoIsUnique(instrumentoNotasEstructuradasDTO.CodigoSbs, instrumentoNotasEstructuradasDTO.IdInstrumento);
                VerifyInstrumentoNotaEstructuradaIsUnique(instrumentoNotasEstructuradasDTO);

                int IdSecuencialFechaInicio = (instrumentoNotasEstructuradasDTO.SecuencialFechaInicio.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoNotasEstructuradasDTO.SecuencialFechaInicio);
                int IdSecuencialFechaVencimiento = (instrumentoNotasEstructuradasDTO.SecuencialFechaVencimiento.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoNotasEstructuradasDTO.SecuencialFechaVencimiento);
                int IdSecuencialFechaLiquidacion = (instrumentoNotasEstructuradasDTO.SecuencialFechaLiquidacion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoNotasEstructuradasDTO.SecuencialFechaLiquidacion);
                instrumentoNotasEstructuradasDTO.NombreInstrumento = string.Format("{0} - {1}", tipoInstrumentoDTO.NombreSbsTipoInstrumento, instrumentoNotasEstructuradasDTO.NombreInstrumento);
                Instrumento instrumento = new Instrumento(instrumentoNotasEstructuradasDTO.IdTipoInstrumento, instrumentoNotasEstructuradasDTO.NombreInstrumento, instrumentoNotasEstructuradasDTO.CodigoSbs,
                    instrumentoNotasEstructuradasDTO.IdMoneda, instrumentoNotasEstructuradasDTO.IdEmisor, tipoInstrumentoDTO.IdGrupoInstrumento, instrumentoNotasEstructuradasDTO.IndCategoria,
                    instrumentoNotasEstructuradasDTO.IndFamilia, indActividad, instrumentoNotasEstructuradasDTO.IdClasificacionRiesgo, instrumentoNotasEstructuradasDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                SaveInstrumento(instrumento);

                InstrumentoNotaEstructurada instrumentoNotasEstructuradas = new InstrumentoNotaEstructurada(
                   instrumento.IdInstrumento, instrumentoNotasEstructuradasDTO.IndTipoSubyacente, instrumentoNotasEstructuradasDTO.IndSubyacente,
                   instrumentoNotasEstructuradasDTO.Nemotecnico, instrumentoNotasEstructuradasDTO.CodigoIsin, instrumentoNotasEstructuradasDTO.IndTipoUnidadColocacion,
                   instrumentoNotasEstructuradasDTO.TieneMonedaDual, instrumentoNotasEstructuradasDTO.IdMonedaDual,
                   instrumentoNotasEstructuradasDTO.MontoColocado, instrumentoNotasEstructuradasDTO.MontoEmitido,
                   instrumentoNotasEstructuradasDTO.ValorNominalInicial, instrumentoNotasEstructuradasDTO.ValorNominalSbs,
                   instrumentoNotasEstructuradasDTO.IndTipoCustodia, IdSecuencialFechaInicio,
                   IdSecuencialFechaVencimiento,
                   instrumentoNotasEstructuradasDTO.PlazoDias, IdSecuencialFechaLiquidacion,
                   instrumentoNotasEstructuradasDTO.PorcentajeCapitalGarantizado, instrumentoNotasEstructuradasDTO.IndValorizacion,
                   instrumentoNotasEstructuradasDTO.TieneMandato,
                   instrumentoNotasEstructuradasDTO.IndClase, instrumentoNotasEstructuradasDTO.Observaciones,
                   instrumentoNotasEstructuradasDTO.LoginActualizacion,
                   instrumentoNotasEstructuradasDTO.IndEmisorSubyacente,
                   instrumentoNotasEstructuradasDTO.IndContraparteSubyacente
                   );

                SaveInstrumentoNotaEstructurada(instrumentoNotasEstructuradas);

                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_RegistrarElemento;
        }
        public string RemoveInstrumentoNotasEstructuradas(int idInstrumento, int idNotaEstructurada)
        {
            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);
            InstrumentoNotaEstructurada persistedNotaEstructurada = iInstrumentoNotaEstructuradaRepository.Get(idNotaEstructurada);

            if (persisted == null || persistedNotaEstructurada == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            var ordenes = iOrdenInversionRepository.GetFiltered(x => x.IdInstrumento == idInstrumento);

            if (ordenes != null && ordenes.Count() > 0)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_NotaEstructurada_Tiene_Dependencias));

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_NotaEstructurada_Tiene_Dependencias));

            iInstrumentoNotaEstructuradaRepository.Remove(persistedNotaEstructurada);
            iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
            iInstrumentoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_EliminarElemento;
        }
        public string AnnulInstrumentoNotasEstructuradas(InstrumentoNotaEstructuradaDTO instrumentoNotasEstructuradasDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoNotasEstructuradasDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoNotasEstructuradasDTO.IdInstrumento);
                InstrumentoNotaEstructurada persistedNotaEstructurada = iInstrumentoNotaEstructuradaRepository.Get(instrumentoNotasEstructuradasDTO.IdNotaEstructurada);

                if (persisted == null || persistedNotaEstructurada == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int idAnulado = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

                Instrumento current = new Instrumento(
                    instrumentoNotasEstructuradasDTO.ComentarioAnulacion,
                    instrumentoNotasEstructuradasDTO.LoginActualizacion);

                persisted.IndActividad = idAnulado;
                persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedNotaEstructurada.LoginActualizacion = current.LoginActualizacion;
                persistedNotaEstructurada.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedNotaEstructurada.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoNotaEstructuradaRepository.Merge(persistedNotaEstructurada, persistedNotaEstructurada);
                iInstrumentoNotaEstructuradaRepository.UnitOfWork.Commit();

                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_AnularElemento;
        }
        public string ActiveInstrumentoNotasEstructuradas(InstrumentoNotaEstructuradaDTO instrumentoNotasEstructuradasDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoNotasEstructuradasDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoNotasEstructuradasDTO.IdInstrumento);
                if (!iAnexoIRubroRepository.Any(p => p.AnexoIRubroDetalleInstrumento.Any(q => q.IdTipoInstrumento == persisted.TipoInstrumento.IdTipoInstrumento && q.IdEmisor == persisted.IdEmisor)))
                    throw new ApplicationException("No tiene detalle en el Anexo III");
                InstrumentoNotaEstructurada persistedNotaEstructurada = iInstrumentoNotaEstructuradaRepository.Get(instrumentoNotasEstructuradasDTO.IdNotaEstructurada);

                if (persisted == null || persistedNotaEstructurada == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoNotasEstructuradasDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoNotasEstructuradasDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;
                Instrumento current = new Instrumento(instrumentoNotasEstructuradasDTO.ComentarioHabilitacion,
                    instrumentoNotasEstructuradasDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedNotaEstructurada.LoginActualizacion = current.LoginActualizacion;
                persistedNotaEstructurada.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedNotaEstructurada.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoNotaEstructuradaRepository.Merge(persistedNotaEstructurada, persistedNotaEstructurada);
                iInstrumentoNotaEstructuradaRepository.UnitOfWork.Commit();

                transactionScope.Complete();
            }
            return mensaje;
        }
        public InstrumentoNotaEstructuradaDTO[] GetAllActiveHabilitadoInstrumentoNotaEstructurada()
        {
            var indHabilitado = this.iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);
            var indVigente = this.iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);

            var instrumentos = iInstrumentoNotaEstructuradaRepository.GetFiltered(x => x.Instrumento.IndHabilitacionRiesgo == indHabilitado && x.Instrumento.IndActividad == indVigente).ToArray();
            return instrumentos.ProjectedAsCollection<InstrumentoNotaEstructuradaDTO>().ToArray();
        }
        #endregion

        #region InstrumentoFondoAlternativoAppService Members
        public InstrumentoFondoAlternativoPagedDTO GetFilteredDataFondoAlternativo(int currentIndexPage, int itemsPerPage, string columnName, bool isAscending, string codigoSbs, int idNombreAdmin, int idFondoAlternativo, int idMoneda, int indEstrategia, int indActividad, int indHabilitacion)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;

            InstrumentoFondoAlternativoPagedDTO response = iInstrumentoDataRepository.GetFilteredDataFondoAlternativo(codigoSbs, idNombreAdmin, idFondoAlternativo, idMoneda, indEstrategia, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);

            foreach (InstrumentoFondoAlternativoListadoDTO item in response.ListaInstrumentoFondoAlternativo)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }

            return response;
        }

        public string UpdateInstrumentoFondoAlternativo(InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO, string loginActualizacion)
        {
            #region Validaciones

            if (instrumentoFondoAlternativoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoDTO"));

            Instrumento currentInstrumento = iInstrumentoRepository.Get(instrumentoFondoAlternativoDTO.IdInstrumento);
            InstrumentoFondoAlternativo currentInstrumentoFondoAlternativo = iInstrumentoFondoAlternativoRepository.Get(instrumentoFondoAlternativoDTO.IdFondoAlternativo);

            if (currentInstrumento == null || currentInstrumentoFondoAlternativo == null)
                throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

            VerifyNombreInstrumentoIsUnique(instrumentoFondoAlternativoDTO.CodigoSbs, instrumentoFondoAlternativoDTO.IdInstrumento);
            VerifyInstrumentoFondoAlternativoIsUnique(instrumentoFondoAlternativoDTO);

            #endregion

            #region Variables

            int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoFondoAlternativoDTO.IndHabilitacionRiesgo);
            int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoFondoAlternativoDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
            int IdSecuencialFechaInicio = (instrumentoFondoAlternativoDTO.SecuencialFechaInicio.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFondoAlternativoDTO.SecuencialFechaInicio);
            int? IdSecuencialFechaIngreso = (instrumentoFondoAlternativoDTO.SecuencialFechaIngreso.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoFondoAlternativoDTO.SecuencialFechaIngreso);
            int? IdSecuencialFechaCierre = (instrumentoFondoAlternativoDTO.SecuencialFechaCierre.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoFondoAlternativoDTO.SecuencialFechaCierre);
            int? IdSecuencialFechaFinalPeriodoInversion = (instrumentoFondoAlternativoDTO.SecuencialFechaFinalPeriodoInversion.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoFondoAlternativoDTO.SecuencialFechaFinalPeriodoInversion);

            #endregion

            #region Cambios Instrumento
            TipoInstrumentoDTO tipoInstrumentoDTO = iTipoInstrumentoAppService.GetById(instrumentoFondoAlternativoDTO.IdTipoInstrumento.Value);
            var emisor = iEntidadAppService.GetById(instrumentoFondoAlternativoDTO.IdEmisor);
            var nombreInstrumento = string.Format("{0} - {1} - {2}", tipoInstrumentoDTO.NombreSbsTipoInstrumento, emisor.NombreEntidad, instrumentoFondoAlternativoDTO.CodigoSbs);

            currentInstrumento.IdTipoInstrumento = instrumentoFondoAlternativoDTO.IdTipoInstrumento;
            currentInstrumento.NombreInstrumento = nombreInstrumento;
            currentInstrumento.CodigoSbs = instrumentoFondoAlternativoDTO.CodigoSbs;
            currentInstrumento.IdMoneda = instrumentoFondoAlternativoDTO.IdMoneda;
            currentInstrumento.IdEmisor = instrumentoFondoAlternativoDTO.IdEmisor;
            currentInstrumento.IdGrupoInstrumento = IdGrupoInstrumento;
            currentInstrumento.IndCategoria = instrumentoFondoAlternativoDTO.IndCategoria;
            currentInstrumento.IndFamilia = instrumentoFondoAlternativoDTO.IndFamilia;
            currentInstrumento.IndActividad = indActividad;
            currentInstrumento.IdClasificacionRiesgo = instrumentoFondoAlternativoDTO.IdClasificacionRiesgo;
            currentInstrumento.LoginActualizacion = loginActualizacion;
            currentInstrumento.IndHabilitacionRiesgo = indHabilitacionRiesgo;
            currentInstrumento.UsuarioActualizacion = Constants.UserSystem;
            currentInstrumento.FechaHoraActualizacion = DateTime.Now;

            #endregion

            #region Cambios Fondo Alternativo

            #region Validar Campos

            var validacionFondoAlternativo = new InstrumentoFondoAlternativo(
                    currentInstrumento.IdInstrumento, instrumentoFondoAlternativoDTO.NombreFondo, instrumentoFondoAlternativoDTO.CodigoIsin,
                    instrumentoFondoAlternativoDTO.Nemotecnico,
                    instrumentoFondoAlternativoDTO.TieneMonedaDual, instrumentoFondoAlternativoDTO.IdMonedaDual,
                    instrumentoFondoAlternativoDTO.IndEstructura, instrumentoFondoAlternativoDTO.IdCustodio, instrumentoFondoAlternativoDTO.IndTipoCustodia,
                    instrumentoFondoAlternativoDTO.ValorNominalInicial, instrumentoFondoAlternativoDTO.ValorNominalSbs,
                    instrumentoFondoAlternativoDTO.IndEstrategia, instrumentoFondoAlternativoDTO.InvierteEnAiv,
                    instrumentoFondoAlternativoDTO.IndValorizacionCuotas, instrumentoFondoAlternativoDTO.NombreAiv,
                    instrumentoFondoAlternativoDTO.DescripcionAiv,
                    instrumentoFondoAlternativoDTO.TieneMandato, IdSecuencialFechaInicio,
                    instrumentoFondoAlternativoDTO.IndPeriodicidadActualizacion, IdSecuencialFechaIngreso,
                    IdSecuencialFechaCierre, IdSecuencialFechaFinalPeriodoInversion,
                   /* instrumentoFondoAlternativoDTO.MontoComprometidoInicial, instrumentoFondoAlternativoDTO.MontoComprometidoActual,*/
                   /* instrumentoFondoAlternativoDTO.IdMontoComprometidoPorFondo, */instrumentoFondoAlternativoDTO.MontoEmitido,
                    instrumentoFondoAlternativoDTO.MontoColocado, instrumentoFondoAlternativoDTO.ClaseCuota,
                 /*   instrumentoFondoAlternativoDTO.MontoFondo,*/ instrumentoFondoAlternativoDTO.FormaPago,
                    instrumentoFondoAlternativoDTO.IndPaisEmision, instrumentoFondoAlternativoDTO.IndOpcionDistribucion,
                    instrumentoFondoAlternativoDTO.IndRegionEmision, instrumentoFondoAlternativoDTO.IndFocoGeografico,
                    instrumentoFondoAlternativoDTO.IndClase, instrumentoFondoAlternativoDTO.IndTipoValor,
                    instrumentoFondoAlternativoDTO.FlagComisionExitoMarginal,
                    instrumentoFondoAlternativoDTO.LoginActualizacion
                    );

            #endregion

            currentInstrumentoFondoAlternativo.NombreFondo = instrumentoFondoAlternativoDTO.NombreFondo;
            currentInstrumentoFondoAlternativo.CodigoIsin = instrumentoFondoAlternativoDTO.CodigoIsin;
            currentInstrumentoFondoAlternativo.Nemotecnico = instrumentoFondoAlternativoDTO.Nemotecnico;
            currentInstrumentoFondoAlternativo.TieneMonedaDual = instrumentoFondoAlternativoDTO.TieneMonedaDual;
            currentInstrumentoFondoAlternativo.IdMonedaDual = (instrumentoFondoAlternativoDTO.TieneMonedaDual) ? instrumentoFondoAlternativoDTO.IdMonedaDual : null;
            currentInstrumentoFondoAlternativo.IndEstructura = instrumentoFondoAlternativoDTO.IndEstructura;
            currentInstrumentoFondoAlternativo.IdCustodio = instrumentoFondoAlternativoDTO.IdCustodio;
            currentInstrumentoFondoAlternativo.IndTipoCustodia = instrumentoFondoAlternativoDTO.IndTipoCustodia;
            currentInstrumentoFondoAlternativo.ValorNominalInicial = instrumentoFondoAlternativoDTO.ValorNominalInicial;
            currentInstrumentoFondoAlternativo.ValorNominalSbs = instrumentoFondoAlternativoDTO.ValorNominalSbs;
            currentInstrumentoFondoAlternativo.IndEstrategia = instrumentoFondoAlternativoDTO.IndEstrategia;
            currentInstrumentoFondoAlternativo.InvierteEnAiv = instrumentoFondoAlternativoDTO.InvierteEnAiv;
            currentInstrumentoFondoAlternativo.IndValorizacionCuotas = instrumentoFondoAlternativoDTO.IndValorizacionCuotas;
            currentInstrumentoFondoAlternativo.NombreAiv = instrumentoFondoAlternativoDTO.NombreAiv;
            currentInstrumentoFondoAlternativo.DescripcionAiv = instrumentoFondoAlternativoDTO.DescripcionAiv;
            currentInstrumentoFondoAlternativo.TieneMandato = instrumentoFondoAlternativoDTO.TieneMandato;
            currentInstrumentoFondoAlternativo.IdSecuencialFechaInicio = IdSecuencialFechaInicio;
            currentInstrumentoFondoAlternativo.IndPeriodicidadActualizacion = instrumentoFondoAlternativoDTO.IndPeriodicidadActualizacion;
            currentInstrumentoFondoAlternativo.IdSecuencialFechaIngreso = IdSecuencialFechaIngreso;
            currentInstrumentoFondoAlternativo.IdSecuencialFechaCierre = IdSecuencialFechaCierre;
            currentInstrumentoFondoAlternativo.IdSecuencialFechaFinalPeriodoInversion = IdSecuencialFechaFinalPeriodoInversion;
            //currentInstrumentoFondoAlternativo.MontoComprometidoInicial = instrumentoFondoAlternativoDTO.MontoComprometidoInicial;
            //currentInstrumentoFondoAlternativo.MontoComprometidoActual = instrumentoFondoAlternativoDTO.MontoComprometidoActual;
            //    currentInstrumentoFondoAlternativo.IdMontoComprometidoPorFondo = instrumentoFondoAlternativoDTO.IdMontoComprometidoPorFondo;
            currentInstrumentoFondoAlternativo.MontoEmitido = instrumentoFondoAlternativoDTO.MontoEmitido;
            currentInstrumentoFondoAlternativo.MontoColocado = instrumentoFondoAlternativoDTO.MontoColocado;
            currentInstrumentoFondoAlternativo.ClaseCuota = instrumentoFondoAlternativoDTO.ClaseCuota;
            //    currentInstrumentoFondoAlternativo.MontoFondo = instrumentoFondoAlternativoDTO.MontoFondo;
            currentInstrumentoFondoAlternativo.FormaPago = instrumentoFondoAlternativoDTO.FormaPago;
            currentInstrumentoFondoAlternativo.IndPaisEmision = instrumentoFondoAlternativoDTO.IndPaisEmision;
            currentInstrumentoFondoAlternativo.IndOpcionDistribucion = instrumentoFondoAlternativoDTO.IndOpcionDistribucion;
            currentInstrumentoFondoAlternativo.IndRegionEmision = instrumentoFondoAlternativoDTO.IndRegionEmision;
            currentInstrumentoFondoAlternativo.IndFocoGeografico = instrumentoFondoAlternativoDTO.IndFocoGeografico;
            currentInstrumentoFondoAlternativo.IndClase = instrumentoFondoAlternativoDTO.IndClase;
            currentInstrumentoFondoAlternativo.IndTipoValor = instrumentoFondoAlternativoDTO.IndTipoValor;
            currentInstrumentoFondoAlternativo.FlagComisionExitoMarginal = instrumentoFondoAlternativoDTO.FlagComisionExitoMarginal;
            currentInstrumentoFondoAlternativo.LoginActualizacion = loginActualizacion;
            currentInstrumentoFondoAlternativo.UsuarioActualizacion = Constants.UserSystem;
            currentInstrumentoFondoAlternativo.FechaHoraActualizacion = DateTime.Now;
            currentInstrumentoFondoAlternativo.NegociaMecanismoCentralizado = instrumentoFondoAlternativoDTO.NegociaMecanismoCentralizado;
            currentInstrumentoFondoAlternativo.CargaInversionIndirectaMoneda = instrumentoFondoAlternativoDTO.CargaInversionIndirectaMoneda;
            ActualizarInstrumentoFondoAlternativoMontoComprometido(currentInstrumentoFondoAlternativo, instrumentoFondoAlternativoDTO.MontosComprometido);
            #endregion

            if (iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.TipoValor, (int)eTipoValor.ComisionExito).Id.Equals(instrumentoFondoAlternativoDTO.IndTipoValor))
            {
                #region EliminandoFondoAlternativoComprometidoYLlamada

                iInstrumentoFondoAlternativoRepository.RemoveFondoAlternativoComisionAdministracionOnCascade(currentInstrumentoFondoAlternativo);

                #endregion

                #region FondoAlternativoTasa

                if (instrumentoFondoAlternativoDTO.TasaRemoved != null)
                {
                    foreach (var tasa in instrumentoFondoAlternativoDTO.TasaRemoved)
                        RemoveFondoAlternativoTasa(instrumentoFondoAlternativoDTO.IdInstrumento, tasa.IdFondoAlternativo, tasa.IdFondoAlternativoTasa);
                }

                if (instrumentoFondoAlternativoDTO.TasaAdded != null)
                {
                    instrumentoFondoAlternativoDTO.TasaList = GetFondoAlternativoTasaByIdFondoAlternativo(instrumentoFondoAlternativoDTO.IdFondoAlternativo).ToArray();
                    foreach (var tasa in instrumentoFondoAlternativoDTO.TasaAdded)
                    {
                        InstrumentoFondoAlternativoTasaDTO fondoAlternativoTasaDTO = new InstrumentoFondoAlternativoTasaDTO();
                        fondoAlternativoTasaDTO.IdFondoAlternativo = instrumentoFondoAlternativoDTO.IdFondoAlternativo;
                        fondoAlternativoTasaDTO.Seccion = tasa.Seccion;
                        fondoAlternativoTasaDTO.TasaHasta = tasa.TasaHasta;
                        fondoAlternativoTasaDTO.TasaValor = tasa.TasaValor;

                        AddNewFondoAlternativoTasa(fondoAlternativoTasaDTO, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                    }
                }

                if (instrumentoFondoAlternativoDTO.TasaUpdated != null)
                {
                    foreach (var tasa in instrumentoFondoAlternativoDTO.TasaUpdated)
                    {
                        UpdateFondoAlternativoTasa(tasa, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                    }
                }
                #endregion

                currentInstrumentoFondoAlternativo.FlagComisionExitoMarginal = instrumentoFondoAlternativoDTO.FlagComisionExitoMarginal;

            }

            if (iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.TipoValor, (int)eTipoValor.ComisionAdministracion).Id.Equals(instrumentoFondoAlternativoDTO.IndTipoValor))
            {
                #region FondoAlternativo Tipo Operacion Capital Cash
                //TODO: se caía porque instrumentoFondoAlternativoDTO.TipoOperacionCapitalCash era null
                if (instrumentoFondoAlternativoDTO.TipoOperacionCapitalCash != null)
                {
                    #region Eliminar Tipo Operacion Capital Cash 

                    var listaTipoOperacionCapitalCashActual = currentInstrumentoFondoAlternativo.ListaFondoAlternativoCapitalCash.ToList();
                    foreach (var tipoOperacionCapitalCashActual in listaTipoOperacionCapitalCashActual)
                    {
                        var tipoOperacionCapitalCashYaNoExiste = instrumentoFondoAlternativoDTO.TipoOperacionCapitalCash.FirstOrDefault(x => x.IdTipoOperacionCapitalCash == tipoOperacionCapitalCashActual.IdTipoOperacionCapitalCash);
                        if (tipoOperacionCapitalCashYaNoExiste == null)
                            iInstrumentoFondoAlternativoRepository.RemoveFondoAlternativoCapitalCash(tipoOperacionCapitalCashActual);
                    }

                    #endregion

                    #region Agregar Tipo Operacion Capital Cash

                    foreach (var tipoOperacion in instrumentoFondoAlternativoDTO.TipoOperacionCapitalCash)
                    {
                        var tipoOperacionCapitalExist = currentInstrumentoFondoAlternativo.ListaFondoAlternativoCapitalCash.FirstOrDefault(x => x.IdTipoOperacionCapitalCash == tipoOperacion.IdTipoOperacionCapitalCash);
                        if (tipoOperacionCapitalExist == null)
                        {
                            InstrumentoFondoAlternativoCapitalCash capitalCash = new InstrumentoFondoAlternativoCapitalCash(tipoOperacion.IdTipoOperacionCapitalCash, indActividad);
                            currentInstrumentoFondoAlternativo.ListaFondoAlternativoCapitalCash.Add(capitalCash);
                        }
                    }

                    #endregion
                }

                #endregion

                #region EliminandoFondoAlternativoTasa

                iInstrumentoFondoAlternativoRepository.RemoveFondoAlternativoComisionExitoOnCascade(currentInstrumentoFondoAlternativo);

                #endregion

                #region FondoAlternativoComprometido

                if (instrumentoFondoAlternativoDTO.ComprometidoRemoved != null)
                {
                    foreach (var comprometido in instrumentoFondoAlternativoDTO.ComprometidoRemoved)
                    {
                        var listaDetalleComprometido = GetFondoAlternativoDetalleComprometidoByIdFondoAlternativoComprometido(comprometido.IdFondoAlternativoComprometido);
                        if (listaDetalleComprometido != null)
                        {
                            foreach (var detalleComprometido in listaDetalleComprometido)
                            {
                                RemoveFondoAlternativoDetalleComprometido(instrumentoFondoAlternativoDTO.IdFondoAlternativo, comprometido.IdFondoAlternativoComprometido, detalleComprometido.IdFondoAlternativoDetalleComprometido);
                            }
                        }
                        RemoveFondoAlternativoComprometido(instrumentoFondoAlternativoDTO.IdInstrumento, comprometido.IdFondoAlternativo, comprometido.IdFondoAlternativoComprometido);
                    }
                }
                if (instrumentoFondoAlternativoDTO.ComprometidoAdded != null)
                {
                    foreach (var comprometido in instrumentoFondoAlternativoDTO.ComprometidoAdded)
                    {
                        InstrumentoFondoAlternativoComprometidoDTO fondoAlternativoComprometidoDTO = new InstrumentoFondoAlternativoComprometidoDTO();
                        fondoAlternativoComprometidoDTO.IdFondoAlternativo = currentInstrumentoFondoAlternativo.IdFondoAlternativo;
                        fondoAlternativoComprometidoDTO.Seccion = comprometido.Seccion;
                        fondoAlternativoComprometidoDTO.AnhioDe = comprometido.AnhioDe;
                        fondoAlternativoComprometidoDTO.AnhioA = comprometido.AnhioA;
                        fondoAlternativoComprometidoDTO.MontoFijo = comprometido.MontoFijo;

                        int id = AddNewFondoAlternativoComprometido(fondoAlternativoComprometidoDTO, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                        if (comprometido.DetalleComprometidoAdded != null)
                        {
                            foreach (var comprometidoDetail in comprometido.DetalleComprometidoAdded)
                            {
                                InstrumentoFondoAlternativoDetalleComprometidoDTO fondoAlternativoDetalleComprometidoDTO = new InstrumentoFondoAlternativoDetalleComprometidoDTO();
                                fondoAlternativoDetalleComprometidoDTO.IdFondoAlternativoComprometido = id;
                                fondoAlternativoDetalleComprometidoDTO.Seccion = comprometidoDetail.Seccion;
                                fondoAlternativoDetalleComprometidoDTO.TasaValor = comprometidoDetail.TasaValor;
                                fondoAlternativoDetalleComprometidoDTO.TasaHasta = comprometidoDetail.TasaHasta;

                                comprometido.DetalleComprometidoList = GetFondoAlternativoDetalleComprometidoByIdFondoAlternativoComprometido(id).ToArray();

                                AddNewFondoAlternativoDetalleComprometido(fondoAlternativoDetalleComprometidoDTO, comprometido, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                            }
                        }
                    }
                }
                if (instrumentoFondoAlternativoDTO.ComprometidoUpdated != null)
                {

                    foreach (var comprometido in instrumentoFondoAlternativoDTO.ComprometidoUpdated)
                    {
                        UpdateFondoAlternativoComprometido(comprometido, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);

                        if (comprometido.DetalleComprometidoAdded != null)
                        {
                            foreach (var comprometidoDetail in comprometido.DetalleComprometidoAdded)
                            {
                                InstrumentoFondoAlternativoDetalleComprometidoDTO fondoAlternativoDetalleComprometidoDTO = new InstrumentoFondoAlternativoDetalleComprometidoDTO();
                                fondoAlternativoDetalleComprometidoDTO.IdFondoAlternativoComprometido = comprometido.IdFondoAlternativoComprometido;
                                fondoAlternativoDetalleComprometidoDTO.Seccion = comprometidoDetail.Seccion;
                                fondoAlternativoDetalleComprometidoDTO.TasaValor = comprometidoDetail.TasaValor;
                                fondoAlternativoDetalleComprometidoDTO.TasaHasta = comprometidoDetail.TasaHasta;

                                comprometido.DetalleComprometidoList = GetFondoAlternativoDetalleComprometidoByIdFondoAlternativoComprometido(comprometido.IdFondoAlternativoComprometido).ToArray();

                                AddNewFondoAlternativoDetalleComprometido(fondoAlternativoDetalleComprometidoDTO, comprometido, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);

                            }
                        }

                        if (comprometido.DetalleComprometidoUpdated != null)
                        {
                            foreach (var comprometidoDetail in comprometido.DetalleComprometidoUpdated)
                            {
                                UpdateFondoAlternativoDetalleComprometido(comprometidoDetail, comprometido, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);

                            }
                        }

                        if (comprometido.DetalleComprometidoRemoved != null)
                        {
                            foreach (var detalleComprometidoList in comprometido.DetalleComprometidoRemoved)
                            {
                                RemoveFondoAlternativoDetalleComprometido(comprometido.IdFondoAlternativo, detalleComprometidoList.IdFondoAlternativoComprometido, detalleComprometidoList.IdFondoAlternativoDetalleComprometido);
                            }
                        }
                    }
                }

                #endregion

                #region FondoAlternativoLlamada

                if (instrumentoFondoAlternativoDTO.LlamadaRemoved != null)
                {
                    foreach (var llamada in instrumentoFondoAlternativoDTO.LlamadaRemoved)
                    {
                        var listaDetalleLlamada = GetFondoAlternativoDetalleLlamadaByIdFondoAlternativoLlamada(llamada.IdFondoAlternativoLlamada);
                        if (listaDetalleLlamada != null)
                        {
                            foreach (var detalleLlamada in listaDetalleLlamada)
                            {
                                RemoveFondoAlternativoDetalleLlamada(instrumentoFondoAlternativoDTO.IdFondoAlternativo, llamada.IdFondoAlternativoLlamada, detalleLlamada.IdFondoAlternativoDetalleLlamada);
                            }
                        }
                        RemoveFondoAlternativoLlamada(instrumentoFondoAlternativoDTO.IdInstrumento, llamada.IdFondoAlternativo, llamada.IdFondoAlternativoLlamada);
                    }
                }

                if (instrumentoFondoAlternativoDTO.LlamadaAdded != null)
                {
                    instrumentoFondoAlternativoDTO.LlamadaList = GetFondoAlternativoLlamadaByIdFondoAlternativo(instrumentoFondoAlternativoDTO.IdFondoAlternativo).ToArray();
                    foreach (var llamada in instrumentoFondoAlternativoDTO.LlamadaAdded)
                    {
                        InstrumentoFondoAlternativoLlamadaDTO fondoAlternativoLlamadaDTO = new InstrumentoFondoAlternativoLlamadaDTO();
                        fondoAlternativoLlamadaDTO.IdFondoAlternativo = currentInstrumentoFondoAlternativo.IdFondoAlternativo;
                        fondoAlternativoLlamadaDTO.Seccion = llamada.Seccion;
                        fondoAlternativoLlamadaDTO.AnhioDe = llamada.AnhioDe;
                        fondoAlternativoLlamadaDTO.AnhioA = llamada.AnhioA;
                        fondoAlternativoLlamadaDTO.MontoFijo = llamada.MontoFijo;

                        int id = AddNewFondoAlternativoLlamada(fondoAlternativoLlamadaDTO, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                        if (llamada.DetalleLlamadaAdded != null)
                        {
                            foreach (var llamadaDetail in llamada.DetalleLlamadaAdded)
                            {
                                InstrumentoFondoAlternativoDetalleLlamadaDTO fondoAlternativoDetalleLlamadaDTO = new InstrumentoFondoAlternativoDetalleLlamadaDTO();
                                fondoAlternativoDetalleLlamadaDTO.IdFondoAlternativoLlamada = id;
                                fondoAlternativoDetalleLlamadaDTO.Seccion = llamadaDetail.Seccion;
                                fondoAlternativoDetalleLlamadaDTO.TasaValor = llamadaDetail.TasaValor;
                                fondoAlternativoDetalleLlamadaDTO.TasaHasta = llamadaDetail.TasaHasta;

                                llamada.DetalleLlamadaList = GetFondoAlternativoDetalleLlamadaByIdFondoAlternativoLlamada(id).ToArray();

                                AddNewFondoAlternativoDetalleLlamada(fondoAlternativoDetalleLlamadaDTO, llamada, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                            }
                        }
                    }
                }
                if (instrumentoFondoAlternativoDTO.LlamadaUpdated != null)
                {

                    foreach (var llamada in instrumentoFondoAlternativoDTO.LlamadaUpdated)
                    {
                        UpdateFondoAlternativoLlamada(llamada, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                        if (llamada.DetalleLlamadaAdded != null)
                        {
                            foreach (var llamadaDetail in llamada.DetalleLlamadaAdded)
                            {
                                InstrumentoFondoAlternativoDetalleLlamadaDTO fondoAlternativoDetalleLlamadaDTO = new InstrumentoFondoAlternativoDetalleLlamadaDTO();
                                fondoAlternativoDetalleLlamadaDTO.IdFondoAlternativoLlamada = llamada.IdFondoAlternativoLlamada;
                                fondoAlternativoDetalleLlamadaDTO.Seccion = llamadaDetail.Seccion;
                                fondoAlternativoDetalleLlamadaDTO.TasaValor = llamadaDetail.TasaValor;
                                fondoAlternativoDetalleLlamadaDTO.TasaHasta = llamadaDetail.TasaHasta;

                                llamada.DetalleLlamadaList = GetFondoAlternativoDetalleLlamadaByIdFondoAlternativoLlamada(llamada.IdFondoAlternativoLlamada).ToArray();

                                AddNewFondoAlternativoDetalleLlamada(fondoAlternativoDetalleLlamadaDTO, llamada, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                            }
                        }

                        if (llamada.DetalleLlamadaUpdated != null)
                        {
                            foreach (var llamadaDetail in llamada.DetalleLlamadaUpdated)
                            {
                                UpdateFondoAlternativoDetalleLlamada(llamadaDetail, llamada, instrumentoFondoAlternativoDTO.LoginActualizacion, currentInstrumento.FechaHoraActualizacion);
                            }
                        }

                        if (llamada.DetalleLlamadaRemoved != null)
                        {
                            foreach (var detalleLlamadaList in llamada.DetalleLlamadaRemoved)
                            {
                                RemoveFondoAlternativoDetalleLlamada(llamada.IdFondoAlternativo, detalleLlamadaList.IdFondoAlternativoLlamada, detalleLlamadaList.IdFondoAlternativoDetalleLlamada);
                            }
                        }
                    }
                }

                #endregion
            }

            iInstrumentoFondoAlternativoRepository.UnitOfWork.Commit();
            iInstrumentoDataRepository.UpdateSeccionAllFondoAlternativo(currentInstrumentoFondoAlternativo.IdFondoAlternativo);

            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public InstrumentoFondoAlternativoDTO GetByIdInstrumentoFondoAlternativo(int idInstrumento, int idFondoAlternativo)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.FondoAlternativo = iInstrumentoFondoAlternativoRepository.FirstOrDefault(x => x.IdInstrumento == idInstrumento);

            if (instrumento.FondoAlternativo == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string SecuencialFechaIngreso = instrumento.FondoAlternativo.IdSecuencialFechaIngreso == null ? string.Empty : Helper.ConvertIdFechaToFechaString(instrumento.FondoAlternativo.IdSecuencialFechaIngreso.Value);
            string SecuencialFechaInicio = Helper.ConvertIdFechaToFechaString(instrumento.FondoAlternativo.IdSecuencialFechaInicio);
            string SecuencialFechaCierre = instrumento.FondoAlternativo.IdSecuencialFechaCierre == null ? string.Empty : Helper.ConvertIdFechaToFechaString(instrumento.FondoAlternativo.IdSecuencialFechaCierre.Value);
            string SecuencialFechaFinalPeriodoInversion = instrumento.FondoAlternativo.IdSecuencialFechaFinalPeriodoInversion == null ? string.Empty : Helper.ConvertIdFechaToFechaString(instrumento.FondoAlternativo.IdSecuencialFechaFinalPeriodoInversion.Value);

            instrumento.FondoAlternativo.SecuencialFechaIngreso = SecuencialFechaIngreso;
            instrumento.FondoAlternativo.SecuencialFechaInicio = SecuencialFechaInicio;
            instrumento.FondoAlternativo.SecuencialFechaCierre = SecuencialFechaCierre;
            instrumento.FondoAlternativo.SecuencialFechaFinalPeriodoInversion = SecuencialFechaFinalPeriodoInversion;

            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO = instrumento.ProjectedAs<InstrumentoFondoAlternativoDTO>();
            instrumentoFondoAlternativoDTO.TipoOperacionCapitalCash = instrumento.FondoAlternativo.ListaFondoAlternativoCapitalCash.ProjectedAsCollection<TipoOperacionCapitalCashItemDTO>().ToArray();
            instrumentoFondoAlternativoDTO.NegociaMecanismoCentralizado = instrumento.FondoAlternativo.NegociaMecanismoCentralizado == null ? false : instrumento.FondoAlternativo.NegociaMecanismoCentralizado.Value;
            instrumentoFondoAlternativoDTO.CargaInversionIndirectaMoneda = instrumento.FondoAlternativo.CargaInversionIndirectaMoneda == null ? false : instrumento.FondoAlternativo.CargaInversionIndirectaMoneda;
            decimal MaxDecimal = (decimal)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXDecimal).ValorAuxDecimal1;
            int MaxInt = (int)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXEntero).ValorAuxNum1;


            if (iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.TipoValor, (int)eTipoValor.ComisionExito).Id.Equals(instrumentoFondoAlternativoDTO.IndTipoValor))
            {
                instrumentoFondoAlternativoDTO.TasaList = GetFondoAlternativoTasaByIdFondoAlternativo(instrumentoFondoAlternativoDTO.IdFondoAlternativo).OrderByDescending(x => x.TasaHasta).ThenByDescending(x => x.TasaValor).ToArray();
                foreach (var tasaList in instrumentoFondoAlternativoDTO.TasaList)
                {
                    if (MaxDecimal.Equals(tasaList.TasaHasta))
                        tasaList.TasaHasta = -1;
                }
                foreach (var capitalCash in instrumento.FondoAlternativo.ListaFondoAlternativoCapitalCash)
                {
                    int idTipoOperacionCapital = (int)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.CapitalCallCashDistribution, capitalCash.IdTipoOperacionCapitalCash).IdIndicador;
                    capitalCash.IdTipoOperacionCapitalCash = idTipoOperacionCapital;
                }
            }

            if (iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.TipoValor, (int)eTipoValor.ComisionAdministracion).Id.Equals(instrumentoFondoAlternativoDTO.IndTipoValor))
            {
                instrumentoFondoAlternativoDTO.ComprometidoList = GetFondoAlternativoComprometidoByIdFondoAlternativo(instrumentoFondoAlternativoDTO.IdFondoAlternativo).OrderBy(x => x.AnhioA).ThenBy(x => x.AnhioDe).ToArray();
                foreach (var ComprometidoList in instrumentoFondoAlternativoDTO.ComprometidoList)
                {
                    if (MaxInt.Equals(ComprometidoList.AnhioA))
                        ComprometidoList.AnhioA = -1;
                }
                foreach (var comprometido in instrumentoFondoAlternativoDTO.ComprometidoList)
                {
                    comprometido.DetalleComprometidoList = GetFondoAlternativoDetalleComprometidoByIdFondoAlternativoComprometido(comprometido.IdFondoAlternativoComprometido).OrderByDescending(x => x.TasaHasta).ThenByDescending(x => x.TasaValor).ToArray();
                    foreach (var DetalleComprometidoList in comprometido.DetalleComprometidoList)
                    {
                        if (MaxDecimal.Equals(DetalleComprometidoList.TasaHasta))
                            DetalleComprometidoList.TasaHasta = -1;
                    }
                }

                instrumentoFondoAlternativoDTO.LlamadaList = GetFondoAlternativoLlamadaByIdFondoAlternativo(instrumentoFondoAlternativoDTO.IdFondoAlternativo).OrderBy(x => x.AnhioA).ThenBy(x => x.AnhioDe).ToArray();
                foreach (var LlamadaList in instrumentoFondoAlternativoDTO.LlamadaList)
                {
                    if (MaxInt.Equals(LlamadaList.AnhioA))
                        LlamadaList.AnhioA = -1;
                }
                foreach (var llamada in instrumentoFondoAlternativoDTO.LlamadaList)
                {
                    llamada.DetalleLlamadaList = GetFondoAlternativoDetalleLlamadaByIdFondoAlternativoLlamada(llamada.IdFondoAlternativoLlamada).OrderByDescending(x => x.TasaHasta).ThenByDescending(x => x.TasaValor).ToArray();
                    foreach (var DetalleLlamadaList in llamada.DetalleLlamadaList)
                    {
                        if (MaxDecimal.Equals(DetalleLlamadaList.TasaHasta))
                            DetalleLlamadaList.TasaHasta = -1;
                    }
                }
            }

            instrumentoFondoAlternativoDTO.MontosComprometido = instrumento.FondoAlternativo.InstrumentoFondoAlternativoMontoComprometido.Select(x => new InstrumentoAlternativoMontoComprometidoDTO
            {
                CodigoFondo = x.FondoPension.CodigoFondo,
                Fondo = x.FondoPension.NombreFondo,
                IdFondo = x.IdFondoPension,
                MontoComprometidoActual = x.MontoComprometidoActual,
                MontoComprometidoInicial = x.MontoComprometidoInicial
            }).ToArray();
            return instrumentoFondoAlternativoDTO;
        }

        public string AddNewInstrumentoFondoAlternativo(InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFondoAlternativoDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);

                VerifyNombreInstrumentoIsUnique(instrumentoFondoAlternativoDTO.CodigoSbs, instrumentoFondoAlternativoDTO.IdInstrumento);
                VerifyInstrumentoFondoAlternativoIsUnique(instrumentoFondoAlternativoDTO);
                TipoInstrumentoDTO tipoInstrumentoDTO = iTipoInstrumentoAppService.GetById(instrumentoFondoAlternativoDTO.IdTipoInstrumento.Value);
                int IdSecuencialFechaInicio = (instrumentoFondoAlternativoDTO.SecuencialFechaInicio.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFondoAlternativoDTO.SecuencialFechaInicio);
                int? IdSecuencialFechaIngreso = (instrumentoFondoAlternativoDTO.SecuencialFechaIngreso.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoFondoAlternativoDTO.SecuencialFechaIngreso);
                int? IdSecuencialFechaCierre = (instrumentoFondoAlternativoDTO.SecuencialFechaCierre.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoFondoAlternativoDTO.SecuencialFechaCierre);
                int? IdSecuencialFechaFinalPeriodoInversion = (instrumentoFondoAlternativoDTO.SecuencialFechaFinalPeriodoInversion.Equals("_")) ? new Nullable<int>() : Helper.ConvertFechaStringToIdFecha(instrumentoFondoAlternativoDTO.SecuencialFechaFinalPeriodoInversion);

                instrumentoFondoAlternativoDTO.NombreInstrumento = string.Format("{0} - {1}", tipoInstrumentoDTO.NombreSbsTipoInstrumento, instrumentoFondoAlternativoDTO.NombreInstrumento);
                var emisor = iEntidadAppService.GetById(instrumentoFondoAlternativoDTO.IdEmisor);
                var nombreInstrumento = string.Format("{0} - {1} - {2}", tipoInstrumentoDTO.NombreSbsTipoInstrumento, emisor.NombreEntidad, instrumentoFondoAlternativoDTO.CodigoSbs);
                Instrumento instrumento = new Instrumento(instrumentoFondoAlternativoDTO.IdTipoInstrumento, nombreInstrumento, instrumentoFondoAlternativoDTO.CodigoSbs,
                    instrumentoFondoAlternativoDTO.IdMoneda, instrumentoFondoAlternativoDTO.IdEmisor, tipoInstrumentoDTO.IdGrupoInstrumento, instrumentoFondoAlternativoDTO.IndCategoria,
                    instrumentoFondoAlternativoDTO.IndFamilia, indActividad, instrumentoFondoAlternativoDTO.IdClasificacionRiesgo, instrumentoFondoAlternativoDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                SaveInstrumento(instrumento);

                InstrumentoFondoAlternativo instrumentoFondoAlternativo = new InstrumentoFondoAlternativo(
                    instrumento.IdInstrumento, instrumentoFondoAlternativoDTO.NombreFondo, instrumentoFondoAlternativoDTO.CodigoIsin,
                    instrumentoFondoAlternativoDTO.Nemotecnico,
                    instrumentoFondoAlternativoDTO.TieneMonedaDual, instrumentoFondoAlternativoDTO.IdMonedaDual,
                    instrumentoFondoAlternativoDTO.IndEstructura, instrumentoFondoAlternativoDTO.IdCustodio, instrumentoFondoAlternativoDTO.IndTipoCustodia,
                    instrumentoFondoAlternativoDTO.ValorNominalInicial, instrumentoFondoAlternativoDTO.ValorNominalSbs,
                    instrumentoFondoAlternativoDTO.IndEstrategia, instrumentoFondoAlternativoDTO.InvierteEnAiv,
                    instrumentoFondoAlternativoDTO.IndValorizacionCuotas, instrumentoFondoAlternativoDTO.NombreAiv,
                    instrumentoFondoAlternativoDTO.DescripcionAiv,
                    instrumentoFondoAlternativoDTO.TieneMandato, IdSecuencialFechaInicio,
                    instrumentoFondoAlternativoDTO.IndPeriodicidadActualizacion, IdSecuencialFechaIngreso,
                    IdSecuencialFechaCierre, IdSecuencialFechaFinalPeriodoInversion,
                  //   instrumentoFondoAlternativoDTO.MontoComprometidoInicial, instrumentoFondoAlternativoDTO.MontoComprometidoActual,
                  /*  instrumentoFondoAlternativoDTO.IdMontoComprometidoPorFondo,*/ instrumentoFondoAlternativoDTO.MontoEmitido,
                    instrumentoFondoAlternativoDTO.MontoColocado, instrumentoFondoAlternativoDTO.ClaseCuota,
                  /*  instrumentoFondoAlternativoDTO.MontoFondo,*/ instrumentoFondoAlternativoDTO.FormaPago,
                    instrumentoFondoAlternativoDTO.IndPaisEmision, instrumentoFondoAlternativoDTO.IndOpcionDistribucion,
                    instrumentoFondoAlternativoDTO.IndRegionEmision, instrumentoFondoAlternativoDTO.IndFocoGeografico,
                    instrumentoFondoAlternativoDTO.IndClase, instrumentoFondoAlternativoDTO.IndTipoValor,
                    instrumentoFondoAlternativoDTO.FlagComisionExitoMarginal,
                    instrumentoFondoAlternativoDTO.LoginActualizacion
                    )
                {
                    NegociaMecanismoCentralizado = instrumentoFondoAlternativoDTO.NegociaMecanismoCentralizado
                };

                instrumentoFondoAlternativo.CargaInversionIndirectaMoneda = instrumentoFondoAlternativoDTO.CargaInversionIndirectaMoneda;
                ActualizarInstrumentoFondoAlternativoMontoComprometido(instrumentoFondoAlternativo, instrumentoFondoAlternativoDTO.MontosComprometido);
                SaveInstrumentoFondoAlternativo(instrumentoFondoAlternativo);


                if (iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.TipoValor, (int)eTipoValor.ComisionExito).Id.Equals(instrumentoFondoAlternativoDTO.IndTipoValor))
                {
                    #region FondoAlternativoTasa

                    if (instrumentoFondoAlternativoDTO.TasaAdded != null)
                    {
                        instrumentoFondoAlternativoDTO.TasaList = GetFondoAlternativoTasaByIdFondoAlternativo(instrumentoFondoAlternativoDTO.IdFondoAlternativo).ToArray();
                        if (instrumentoFondoAlternativoDTO.TasaAdded != null)
                        {
                            foreach (var tasa in instrumentoFondoAlternativoDTO.TasaAdded)
                            {
                                InstrumentoFondoAlternativoTasaDTO fondoAlternativoTasaDTO = new InstrumentoFondoAlternativoTasaDTO();
                                fondoAlternativoTasaDTO.IdFondoAlternativo = instrumentoFondoAlternativo.IdFondoAlternativo;
                                fondoAlternativoTasaDTO.Seccion = tasa.Seccion;
                                fondoAlternativoTasaDTO.TasaHasta = tasa.TasaHasta;
                                fondoAlternativoTasaDTO.TasaValor = tasa.TasaValor;

                                AddNewFondoAlternativoTasa(fondoAlternativoTasaDTO, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, instrumento.FechaHoraActualizacion);
                            }
                        }
                    }

                    #endregion
                }

                if (iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.TipoValor, (int)eTipoValor.ComisionAdministracion).Id.Equals(instrumentoFondoAlternativoDTO.IndTipoValor))
                {
                    #region FondoAlternativo Tipo Operacion Capital Cash

                    instrumentoFondoAlternativo.FlagComisionExitoMarginal = instrumentoFondoAlternativoDTO.FlagComisionExitoMarginal;

                    if (instrumentoFondoAlternativoDTO.TipoOperacionCapitalCash != null)
                    {
                        foreach (var tipoOperacion in instrumentoFondoAlternativoDTO.TipoOperacionCapitalCash)
                        {
                            InstrumentoFondoAlternativoCapitalCash capitalCash = new InstrumentoFondoAlternativoCapitalCash(tipoOperacion.IdTipoOperacionCapitalCash, indActividad);
                            instrumentoFondoAlternativo.ListaFondoAlternativoCapitalCash.Add(capitalCash);
                        }
                    }

                    #endregion

                    #region FondoAlternativoComprometido


                    if (instrumentoFondoAlternativoDTO.ComprometidoAdded != null)
                    {
                        instrumentoFondoAlternativoDTO.ComprometidoList = GetFondoAlternativoComprometidoByIdFondoAlternativo(instrumentoFondoAlternativoDTO.IdFondoAlternativo).ToArray();

                        foreach (var comprometido in instrumentoFondoAlternativoDTO.ComprometidoAdded)
                        {
                            InstrumentoFondoAlternativoComprometidoDTO fondoAlternativoComprometidoDTO = new InstrumentoFondoAlternativoComprometidoDTO();
                            fondoAlternativoComprometidoDTO.IdFondoAlternativo = instrumentoFondoAlternativo.IdFondoAlternativo;
                            fondoAlternativoComprometidoDTO.Seccion = comprometido.Seccion;
                            fondoAlternativoComprometidoDTO.AnhioDe = comprometido.AnhioDe;
                            fondoAlternativoComprometidoDTO.AnhioA = comprometido.AnhioA;
                            fondoAlternativoComprometidoDTO.MontoFijo = comprometido.MontoFijo;

                            int id = AddNewFondoAlternativoComprometido(fondoAlternativoComprometidoDTO, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, instrumento.FechaHoraActualizacion);
                            if (comprometido.DetalleComprometidoAdded != null)
                            {
                                foreach (var comprometidoDetail in comprometido.DetalleComprometidoAdded)
                                {
                                    InstrumentoFondoAlternativoDetalleComprometidoDTO fondoAlternativoDetalleComprometidoDTO = new InstrumentoFondoAlternativoDetalleComprometidoDTO();
                                    fondoAlternativoDetalleComprometidoDTO.IdFondoAlternativoComprometido = id;
                                    fondoAlternativoDetalleComprometidoDTO.Seccion = comprometidoDetail.Seccion;
                                    fondoAlternativoDetalleComprometidoDTO.TasaValor = comprometidoDetail.TasaValor;
                                    fondoAlternativoDetalleComprometidoDTO.TasaHasta = comprometidoDetail.TasaHasta;

                                    comprometido.DetalleComprometidoList = GetFondoAlternativoDetalleComprometidoByIdFondoAlternativoComprometido(id).ToArray();


                                    AddNewFondoAlternativoDetalleComprometido(fondoAlternativoDetalleComprometidoDTO, comprometido, instrumentoFondoAlternativoDTO.LoginActualizacion, instrumento.FechaHoraActualizacion);
                                }
                            }
                        }
                    }
                    #endregion

                    #region FondoAlternativoLlamada

                    if (instrumentoFondoAlternativoDTO.LlamadaAdded != null)
                    {
                        instrumentoFondoAlternativoDTO.LlamadaList = GetFondoAlternativoLlamadaByIdFondoAlternativo(instrumentoFondoAlternativoDTO.IdFondoAlternativo).ToArray();
                        foreach (var llamada in instrumentoFondoAlternativoDTO.LlamadaAdded)
                        {
                            InstrumentoFondoAlternativoLlamadaDTO fondoAlternativoLlamadaDTO = new InstrumentoFondoAlternativoLlamadaDTO();
                            fondoAlternativoLlamadaDTO.IdFondoAlternativo = instrumentoFondoAlternativo.IdFondoAlternativo;
                            fondoAlternativoLlamadaDTO.Seccion = llamada.Seccion;
                            fondoAlternativoLlamadaDTO.AnhioDe = llamada.AnhioDe;
                            fondoAlternativoLlamadaDTO.AnhioA = llamada.AnhioA;


                            int id = AddNewFondoAlternativoLlamada(fondoAlternativoLlamadaDTO, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, instrumento.FechaHoraActualizacion);
                            if (llamada.DetalleLlamadaAdded != null)
                            {
                                foreach (var llamadaDetail in llamada.DetalleLlamadaAdded)
                                {
                                    InstrumentoFondoAlternativoDetalleLlamadaDTO fondoAlternativoDetalleLlamadaDTO = new InstrumentoFondoAlternativoDetalleLlamadaDTO();
                                    fondoAlternativoDetalleLlamadaDTO.IdFondoAlternativoLlamada = id;
                                    fondoAlternativoDetalleLlamadaDTO.Seccion = llamadaDetail.Seccion;
                                    fondoAlternativoDetalleLlamadaDTO.TasaValor = llamadaDetail.TasaValor;
                                    fondoAlternativoDetalleLlamadaDTO.TasaHasta = llamadaDetail.TasaHasta;

                                    llamada.DetalleLlamadaList = GetFondoAlternativoDetalleLlamadaByIdFondoAlternativoLlamada(id).ToArray();

                                    AddNewFondoAlternativoDetalleLlamada(fondoAlternativoDetalleLlamadaDTO, llamada, instrumentoFondoAlternativoDTO.LoginActualizacion, instrumento.FechaHoraActualizacion);
                                }
                            }
                        }
                    }
                    #endregion
                }
                iInstrumentoDataRepository.UpdateSeccionAllFondoAlternativo(instrumentoFondoAlternativo.IdFondoAlternativo);
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_RegistrarElemento;
        }
        private void ActualizarInstrumentoFondoAlternativoMontoComprometido(InstrumentoFondoAlternativo InstrumentoAlternativo, InstrumentoAlternativoMontoComprometidoDTO[] MontosComprometido)
        {
            var totalMontoInicial = MontosComprometido.Sum(x => x.MontoComprometidoInicial);
            var totalMontoActual = MontosComprometido.Sum(x => x.MontoComprometidoActual);

            /* if (totalMontoInicial > InstrumentoAlternativo.MontoComprometidoInicial)
                 throw new ApplicationException("Los monto   s comprometidos por fondo no pueden ser mayores que los montos comprometidos iniciales.");

             if (totalMontoActual > InstrumentoAlternativo.MontoComprometidoActual)
                 throw new ApplicationException("Los montos comprometidos por fondo no pueden ser mayores que los montos comprometidos actuales.");
                 */
            foreach (var item in MontosComprometido)
            {
                var montoComprometido = InstrumentoAlternativo.InstrumentoFondoAlternativoMontoComprometido.FirstOrDefault(x => x.IdFondoPension == item.IdFondo);

                if ((item.MontoComprometidoActual == null && item.MontoComprometidoInicial == null) || (item.MontoComprometidoActual <= 0 && item.MontoComprometidoInicial <= 0))//INC-19-1134 mcalla
                {
                    if (montoComprometido != null)
                        iInstrumentoFondoAlternativoRepository.RemoveFondoAlternativoMontoComprometido(montoComprometido);

                    continue;
                }

                if (montoComprometido == null)
                {
                    montoComprometido = new InstrumentoFondoAlternativoMontoComprometido();
                    InstrumentoAlternativo.InstrumentoFondoAlternativoMontoComprometido.Add(montoComprometido);
                }
                montoComprometido.IdFondoPension = item.IdFondo;
                montoComprometido.MontoComprometidoActual = item.MontoComprometidoActual;
                montoComprometido.MontoComprometidoInicial = item.MontoComprometidoInicial;
            }
        }
        public string RemoveInstrumentoFondoAlternativo(int idInstrumento, int idFondoAlternativo)
        {

            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);
            InstrumentoFondoAlternativo persistedFondoAlternativo = iInstrumentoFondoAlternativoRepository.Get(idFondoAlternativo);

            if (persisted == null || persistedFondoAlternativo == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_FondoAlternativo_Tiene_Dependencias));

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO = GetByIdInstrumentoFondoAlternativo(idInstrumento, idFondoAlternativo);

                if (iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.TipoValor, (int)eTipoValor.ComisionExito).Id.Equals(instrumentoFondoAlternativoDTO.IndTipoValor))
                {
                    #region FondoAlternativoTasa

                    if (instrumentoFondoAlternativoDTO.TasaList != null)
                    {
                        foreach (var tasaList in instrumentoFondoAlternativoDTO.TasaList)
                            RemoveFondoAlternativoTasa(idInstrumento, idFondoAlternativo, tasaList.IdFondoAlternativoTasa);
                    }

                    #endregion
                }


                if (iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.TipoValor, (int)eTipoValor.ComisionAdministracion).Id.Equals(instrumentoFondoAlternativoDTO.IndTipoValor))
                {
                    #region FondoAlternativoComprometido

                    if (instrumentoFondoAlternativoDTO.ComprometidoList != null)
                    {
                        foreach (var comprometidoList in instrumentoFondoAlternativoDTO.ComprometidoList)
                        {
                            if (comprometidoList.DetalleComprometidoList != null)
                            {
                                foreach (var detalleComprometidoList in comprometidoList.DetalleComprometidoList)
                                {
                                    RemoveFondoAlternativoDetalleComprometido(comprometidoList.IdFondoAlternativo, comprometidoList.IdFondoAlternativoComprometido, detalleComprometidoList.IdFondoAlternativoDetalleComprometido);
                                }
                            }
                            RemoveFondoAlternativoComprometido(idInstrumento, idFondoAlternativo, comprometidoList.IdFondoAlternativoComprometido);
                        }
                    }
                    #endregion

                    #region FondoAlternativoLlamada

                    if (instrumentoFondoAlternativoDTO.LlamadaList != null)
                    {
                        foreach (var llamadaList in instrumentoFondoAlternativoDTO.LlamadaList)
                        {
                            if (llamadaList.DetalleLlamadaList != null)
                            {
                                foreach (var detalleLlamadaList in llamadaList.DetalleLlamadaList)
                                {
                                    RemoveFondoAlternativoDetalleLlamada(instrumentoFondoAlternativoDTO.IdFondoAlternativo, llamadaList.IdFondoAlternativoLlamada, detalleLlamadaList.IdFondoAlternativoDetalleLlamada);
                                }
                            }
                            RemoveFondoAlternativoLlamada(idInstrumento, idFondoAlternativo, llamadaList.IdFondoAlternativoLlamada);
                        }
                    }

                    #endregion
                }

                #region InstrumentoFondoAlternativo

                iInstrumentoFondoAlternativoRepository.RemoveListaFondoAlternativoCapitalCashOnCascade(persistedFondoAlternativo);
                iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
                iInstrumentoRepository.UnitOfWork.Commit();

                #endregion
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_EliminarElemento;
        }
        public string AnnulInstrumentoFondoAlternativo(InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFondoAlternativoDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoFondoAlternativoDTO.IdInstrumento);
                InstrumentoFondoAlternativo persistedFondoAlternativo = iInstrumentoFondoAlternativoRepository.Get(instrumentoFondoAlternativoDTO.IdFondoAlternativo);

                if (persisted == null || persistedFondoAlternativo == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int idAnulado = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

                Instrumento current = new Instrumento(instrumentoFondoAlternativoDTO.ComentarioAnulacion,
                    instrumentoFondoAlternativoDTO.LoginActualizacion);

                persisted.IndActividad = idAnulado;
                persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedFondoAlternativo.LoginActualizacion = current.LoginActualizacion;
                persistedFondoAlternativo.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedFondoAlternativo.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoFondoAlternativoRepository.Merge(persistedFondoAlternativo, persistedFondoAlternativo);
                iInstrumentoFondoAlternativoRepository.UnitOfWork.Commit();

                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_AnularElemento;
        }
        public InstrumentoFondoAlternativoDTO[] GetAllFondoAlternativoByGrupo(string grupo)
        {
            GrupoInstrumentoDTO grupoInstrumentoDTO = iGrupoInstrumentoAppService.GetByGrupo(grupo);
            if (grupoInstrumentoDTO == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            IEnumerable<Instrumento> instrumentos = iInstrumentoRepository.GetFiltered(obj => obj.IdGrupoInstrumento.Value.Equals(grupoInstrumentoDTO.IdGrupoInstrumento)).ToList();

            if (instrumentos == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            foreach (Instrumento instrumento in instrumentos)
            {
                instrumento.FondoAlternativo = iInstrumentoFondoAlternativoRepository.FirstOrDefault(obj => obj.IdInstrumento.Equals(instrumento.IdInstrumento));
            }

            return instrumentos.ProjectedAsCollection<InstrumentoFondoAlternativoDTO>().ToArray();
        }
        public List<InstrumentoFondoAlternativoDTO> GetAllFondoMutuoAlternativoHabilitadosPorFecha(string fechaConsulta)
        {
            var dateTimeObj = Helper.ConvertToDateTime(fechaConsulta, "dd-MM-yyyy").AddDays(1).AddTicks(-1); ;
            int indActividadVigente = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            int indHabilitado = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);

            var instrumentos = iInstrumentoFondoAlternativoRepository.GetFiltered(i =>
                i.Instrumento.IndActividad == indActividadVigente &&
                i.Instrumento.IndHabilitacionRiesgo == indHabilitado);

            return instrumentos.ProjectedAsCollection<InstrumentoFondoAlternativoDTO>().ToList();
        }

        public List<InstrumentoFondoAlternativoConEmisorDTO> GetAllFondoMutuoAlternativoHabilitadosPorRangoFecha(string fechaDesde, string fechaHasta)
        {
            DateTime? dateTimeObj_desde = string.IsNullOrEmpty(fechaDesde) ? (DateTime?)null : Helper.ConvertToDateTime(fechaDesde, "dd-MM-yyyy");
            DateTime? dateTimeObj_hasta = string.IsNullOrEmpty(fechaHasta) ? (DateTime?)null : Helper.ConvertToDateTime(fechaHasta, "dd-MM-yyyy");
            int indActividadVigente = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            int indHabilitado = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);

            var instrumentos = iInstrumentoFondoAlternativoRepository.GetFiltered(i => i.Instrumento.IndActividad == indActividadVigente &&
                                                                                       i.Instrumento.IndHabilitacionRiesgo == indHabilitado &&
                                                            ((i.FechaHoraActualizacion >= dateTimeObj_desde || !dateTimeObj_desde.HasValue) &&
                                                            (i.FechaHoraActualizacion <= dateTimeObj_hasta || !dateTimeObj_hasta.HasValue)));

            var result = instrumentos.Select(x => new InstrumentoFondoAlternativoConEmisorDTO()
            {
                Nemotecnico = x.Nemotecnico,
                CodigoIsin = x.CodigoIsin,
                CodigoSbs = x.Instrumento != null ? x.Instrumento.CodigoSbs : string.Empty,
                FechaHoraActualizacion = x.FechaHoraActualizacion,
                IdInstrumento = x.IdInstrumento,
                IdFondoAlternativo = x.IdFondoAlternativo,
                NombreFondo = x.NombreFondo,
                NombreEntidad = x.Instrumento != null ? x.Instrumento.Emisor.NombreEntidad : string.Empty,
                IdEntidad = x.Instrumento != null ? x.Instrumento.Emisor.IdEntidad : 0,
                IdCustodio = x.IdCustodio,
                IdEmisor = x.Instrumento != null ? x.Instrumento.Emisor.IdEntidad : 0
            }).ToList();

            return result;
        }

        public InstrumentoFondoAlternativoConEmisorDTO[] GetAllFondoAlternativo()
        {
            int indActividadVigente = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            int indHabilitado = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);

            var instrumentos = iInstrumentoFondoAlternativoRepository.GetFiltered(i => i.Instrumento.IndActividad == indActividadVigente &&
                                                                                       i.Instrumento.IndHabilitacionRiesgo == indHabilitado);

            //return instrumentos.ProjectedAsCollection<InstrumentoFondoAlternativoDTO>().ToArray();

            var result = instrumentos.Select(x => new InstrumentoFondoAlternativoConEmisorDTO()
            {
                Nemotecnico = x.Nemotecnico,
                CodigoIsin = x.CodigoIsin,
                CodigoSbs = x.Instrumento != null ? x.Instrumento.CodigoSbs : string.Empty,
                FechaHoraActualizacion = x.FechaHoraActualizacion,
                IdInstrumento = x.IdInstrumento,
                IdFondoAlternativo = x.IdFondoAlternativo,
                NombreFondo = x.NombreFondo,
                NombreEntidad = x.Instrumento != null ? x.Instrumento.Emisor.NombreEntidad : string.Empty,
                IdEntidad = x.Instrumento != null ? x.Instrumento.Emisor.IdEntidad : 0,
                IdCustodio = x.IdCustodio,
                IdEmisor = x.Instrumento != null ? x.Instrumento.Emisor.IdEntidad : 0,
                IdMoneda = x.Instrumento.IdMoneda,
            }).ToArray();

            return result;
        }

        public InstrumentoFondoAlternativoDetalleDTO[] GetAllFondoAlternativoDetalle()
        {
            int indActividadVigente = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
            int indHabilitado = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.Habilitado);

            var instrumentos = iInstrumentoFondoAlternativoRepository.GetFiltered(i => i.Instrumento.IndActividad == indActividadVigente &&
                                                                                       i.Instrumento.IndHabilitacionRiesgo == indHabilitado);

            var result = instrumentos.Select(x => new InstrumentoFondoAlternativoDetalleDTO()
            {
                IdInstrumento = x.Instrumento.IdInstrumento,
                NombreInstrumento = x.Instrumento.NombreInstrumento,
                IdEmisor = x.Instrumento.Emisor.IdEntidad,
                NombreEmisor = x.Instrumento.Emisor.NombreEntidad,
                Nemotecnico = x.Nemotecnico,
                CodigoSbs = x.Instrumento.CodigoSbs,
                CodigoIsin = x.CodigoIsin,
                IdTipoInstrumento = x.Instrumento.TipoInstrumento.IdTipoInstrumento,
                NombreTipoInstrumento = x.Instrumento.TipoInstrumento.NombreSbsTipoInstrumento,
                IdCustodio = x.IdCustodio,
                IndPaisEmision = x.IndPaisEmision,
                ValorNominalSbs = x.ValorNominalSbs,
                ValorNominalInicial = x.ValorNominalInicial
            });

            return result.ToArray();
        }

        public string ActiveInstrumentoFondoAlternativo(InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFondoAlternativoDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoFondoAlternativoDTO.IdInstrumento);
                if (!iAnexoIRubroRepository.Any(p => p.AnexoIRubroDetalleInstrumento.Any(q => q.IdTipoInstrumento == persisted.TipoInstrumento.IdTipoInstrumento && q.IdEmisor == persisted.IdEmisor)))
                    throw new ApplicationException("No tiene detalle en el Anexo III");
                InstrumentoFondoAlternativo persistedFondoAlternativo = iInstrumentoFondoAlternativoRepository.Get(instrumentoFondoAlternativoDTO.IdFondoAlternativo);

                if (persisted == null || persistedFondoAlternativo == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoFondoAlternativoDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoFondoAlternativoDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;

                Instrumento current = new Instrumento(instrumentoFondoAlternativoDTO.ComentarioHabilitacion,
                    instrumentoFondoAlternativoDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedFondoAlternativo.LoginActualizacion = current.LoginActualizacion;
                persistedFondoAlternativo.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedFondoAlternativo.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoFondoAlternativoRepository.Merge(persistedFondoAlternativo, persistedFondoAlternativo);
                iInstrumentoFondoAlternativoRepository.UnitOfWork.Commit();

                transactionScope.Complete();
            }
            return mensaje;
        }
        public ListadoGeneralDTO[] GetAllTramoComprometido()
        {
            List<InstrumentoFondoAlternativoComprometido> parametros = iFondoAlternativoComprometidoRepository.GetAll().ToList();
            ListadoGeneralDTO[] listadoGeneral = parametros.Select(x => new ListadoGeneralDTO
            {
                Item = x.IdFondoAlternativoComprometido,
                TextItem = x.Seccion
            }).ToArray();

            return listadoGeneral;
        }
        public ListadoGeneralDTO[] GetAllTramoLlamada()
        {
            List<InstrumentoFondoAlternativoLlamada> parametros = iFondoAlternativoLlamadaRepository.GetAll().ToList();
            ListadoGeneralDTO[] listadoGeneral = parametros.Select(x => new ListadoGeneralDTO
            {
                Item = x.IdFondoAlternativoLlamada,
                TextItem = x.Seccion
            }).ToArray();

            return listadoGeneral;
        }

        #endregion

        #region InstrumentoFuturoAppService Members
        public InstrumentoFuturoPagedDTO GetFilteredDataFuturo(string codigoSbs, int indTipoSubyacente, int indSubyacente, int indTipoFuturo,
            string secuencialFechaValuationDate, int indActividad, int indHabilitacion, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;
            int idSecuencialFechaValuationDate = (secuencialFechaValuationDate.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(secuencialFechaValuationDate);
            InstrumentoFuturoPagedDTO response = iInstrumentoDataRepository.GetFilteredDataFuturo(codigoSbs, indTipoSubyacente, indSubyacente, indTipoFuturo, idSecuencialFechaValuationDate, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);
            foreach (InstrumentoFuturoListadoDTO item in response.ListaInstrumentoFuturo)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }
            return response;
        }
        public string UpdateInstrumentoFuturo(InstrumentoFuturoDTO instrumentoFuturoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFuturoDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFuturo"));

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoFuturoDTO.IdInstrumento);
                InstrumentoFuturo persistedFuturo = iInstrumentoFuturoRepository.Get(instrumentoFuturoDTO.IdFuturo);

                if (persisted == null || persistedFuturo == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                VerifyNombreInstrumentoIsUnique(instrumentoFuturoDTO.CodigoSbs, instrumentoFuturoDTO.IdInstrumento);
                VerifyInstrumentoFuturoIsUnique(instrumentoFuturoDTO);

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoFuturoDTO.IndHabilitacionRiesgo);

                int IdSecuencialFechaEmision = (instrumentoFuturoDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaEmision);
                int IdSecuencialFechaFirstNotice = (instrumentoFuturoDTO.SecuencialFechaFirstNotice.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaFirstNotice);
                int IdSecuencialFechaFirstTradeDate = (instrumentoFuturoDTO.SecuencialFechaFirstTradeDate.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaFirstTradeDate);
                int IdSecuencialFechaLastTradeDate = (instrumentoFuturoDTO.SecuencialFechaLastTradeDate.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaLastTradeDate);
                int IdSecuencialFechaFirstDelivery = (instrumentoFuturoDTO.SecuencialFechaFirstDelivery.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaFirstDelivery);
                int IdSecuencialFechaLastDelivery = (instrumentoFuturoDTO.SecuencialFechaLastDelivery.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaLastDelivery);
                int IdSecuencialFechaValuationDate = (instrumentoFuturoDTO.SecuencialFechaValuationDate.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaValuationDate);
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoFuturoDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                Instrumento current = new Instrumento(instrumentoFuturoDTO.IdTipoInstrumento, instrumentoFuturoDTO.NombreInstrumento, instrumentoFuturoDTO.CodigoSbs,
                    instrumentoFuturoDTO.IdMoneda, instrumentoFuturoDTO.IdEmisor, IdGrupoInstrumento, instrumentoFuturoDTO.IndCategoria,
                    instrumentoFuturoDTO.IndFamilia, indActividad, instrumentoFuturoDTO.IdClasificacionRiesgo, instrumentoFuturoDTO.LoginActualizacion,
                    indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, current);
                iInstrumentoRepository.UnitOfWork.Commit();

                InstrumentoFuturo instrumentoFuturo = new InstrumentoFuturo(instrumentoFuturoDTO.IdInstrumento, instrumentoFuturoDTO.IndTipoSubyacente,
                    instrumentoFuturoDTO.IndSubyacente, instrumentoFuturoDTO.Ticker,
                    IdSecuencialFechaEmision, IdSecuencialFechaFirstNotice, IdSecuencialFechaFirstTradeDate,
                    IdSecuencialFechaLastTradeDate, IdSecuencialFechaFirstDelivery, IdSecuencialFechaLastDelivery,
                    IdSecuencialFechaValuationDate, instrumentoFuturoDTO.IndTipoLiquidacion, instrumentoFuturoDTO.IdMonedaContrato,
                    instrumentoFuturoDTO.TamanioContrato, instrumentoFuturoDTO.ContratosCirculacion, instrumentoFuturoDTO.Valor1pt, instrumentoFuturoDTO.LotSizeOverride,
                    instrumentoFuturoDTO.IdMonedaPrimaPactada, instrumentoFuturoDTO.TickSize, instrumentoFuturoDTO.Conversion, instrumentoFuturoDTO.TasaInteres,
                    instrumentoFuturoDTO.TieneMandato, instrumentoFuturoDTO.LoginActualizacion);

                instrumentoFuturo.IdFuturo = persistedFuturo.IdFuturo;
                iInstrumentoFuturoRepository.Merge(persistedFuturo, instrumentoFuturo);
                iInstrumentoFuturoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public InstrumentoFuturoDTO GetByIdInstrumentoFuturo(int idInstrumento, int idFuturo)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.Futuro = iInstrumentoFuturoRepository.Get(idFuturo);

            if (instrumento.Futuro == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string SecuencialFechaEmision = Helper.ConvertIdFechaToFechaString(instrumento.Futuro.IdSecuencialFechaEmision);
            string SecuencialFechaFirstNotice = Helper.ConvertIdFechaToFechaString(instrumento.Futuro.IdSecuencialFechaFirstNotice);
            string SecuencialFechaFirstTradeDate = Helper.ConvertIdFechaToFechaString(instrumento.Futuro.IdSecuencialFechaFirstTradeDate);
            string SecuencialFechaLastTradeDate = Helper.ConvertIdFechaToFechaString(instrumento.Futuro.IdSecuencialFechaLastTradeDate);
            string SecuencialFechaFirstDelivery = Helper.ConvertIdFechaToFechaString(instrumento.Futuro.IdSecuencialFechaFirstDelivery);
            string SecuencialFechaLastDelivery = Helper.ConvertIdFechaToFechaString(instrumento.Futuro.IdSecuencialFechaLastDelivery);
            string SecuencialFechaValuationDate = Helper.ConvertIdFechaToFechaString(instrumento.Futuro.IdSecuencialFechaValuationDate);

            instrumento.Futuro.SecuencialFechaEmision = SecuencialFechaEmision;
            instrumento.Futuro.SecuencialFechaFirstNotice = SecuencialFechaFirstNotice;
            instrumento.Futuro.SecuencialFechaFirstTradeDate = SecuencialFechaFirstTradeDate;
            instrumento.Futuro.SecuencialFechaLastTradeDate = SecuencialFechaLastTradeDate;
            instrumento.Futuro.SecuencialFechaFirstDelivery = SecuencialFechaFirstDelivery;
            instrumento.Futuro.SecuencialFechaLastDelivery = SecuencialFechaLastDelivery;
            instrumento.Futuro.SecuencialFechaValuationDate = SecuencialFechaValuationDate;

            return instrumento.ProjectedAs<InstrumentoFuturoDTO>();
        }
        public string AddNewInstrumentoFuturo(InstrumentoFuturoDTO instrumentoFuturoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFuturoDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);
                int idGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoFuturoDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;

                VerifyNombreInstrumentoIsUnique(instrumentoFuturoDTO.CodigoSbs, instrumentoFuturoDTO.IdInstrumento);
                VerifyInstrumentoFuturoIsUnique(instrumentoFuturoDTO);

                int IdSecuencialFechaEmision = (instrumentoFuturoDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaEmision);
                int IdSecuencialFechaFirstNotice = (instrumentoFuturoDTO.SecuencialFechaFirstNotice.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaFirstNotice);
                int IdSecuencialFechaFirstTradeDate = (instrumentoFuturoDTO.SecuencialFechaFirstTradeDate.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaFirstTradeDate);
                int IdSecuencialFechaLastTradeDate = (instrumentoFuturoDTO.SecuencialFechaLastTradeDate.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaLastTradeDate);
                int IdSecuencialFechaFirstDelivery = (instrumentoFuturoDTO.SecuencialFechaFirstDelivery.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaFirstDelivery);
                int IdSecuencialFechaLastDelivery = (instrumentoFuturoDTO.SecuencialFechaLastDelivery.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaLastDelivery);
                int IdSecuencialFechaValuationDate = (instrumentoFuturoDTO.SecuencialFechaValuationDate.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoFuturoDTO.SecuencialFechaValuationDate);

                Instrumento instrumento = new Instrumento(instrumentoFuturoDTO.IdTipoInstrumento, instrumentoFuturoDTO.NombreInstrumento, instrumentoFuturoDTO.CodigoSbs,
                    instrumentoFuturoDTO.IdMoneda, instrumentoFuturoDTO.IdEmisor, idGrupoInstrumento, instrumentoFuturoDTO.IndCategoria,
                    instrumentoFuturoDTO.IndFamilia, indActividad, instrumentoFuturoDTO.IdClasificacionRiesgo, instrumentoFuturoDTO.LoginActualizacion,
                    indHabilitacionRiesgo);
                SaveInstrumento(instrumento);

                InstrumentoFuturo instrumentoFuturo = new InstrumentoFuturo(instrumento.IdInstrumento, instrumentoFuturoDTO.IndTipoSubyacente,
                    instrumentoFuturoDTO.IndSubyacente, instrumentoFuturoDTO.Ticker,
                    IdSecuencialFechaEmision, IdSecuencialFechaFirstNotice, IdSecuencialFechaFirstTradeDate,
                    IdSecuencialFechaLastTradeDate, IdSecuencialFechaFirstDelivery, IdSecuencialFechaLastDelivery,
                    IdSecuencialFechaValuationDate, instrumentoFuturoDTO.IndTipoLiquidacion, instrumentoFuturoDTO.IdMonedaContrato,
                    instrumentoFuturoDTO.TamanioContrato, instrumentoFuturoDTO.ContratosCirculacion, instrumentoFuturoDTO.Valor1pt, instrumentoFuturoDTO.LotSizeOverride,
                    instrumentoFuturoDTO.IdMonedaPrimaPactada, instrumentoFuturoDTO.TickSize, instrumentoFuturoDTO.Conversion, instrumentoFuturoDTO.TasaInteres,
                    instrumentoFuturoDTO.TieneMandato, instrumentoFuturoDTO.LoginActualizacion);

                SaveInstrumentoFuturo(instrumentoFuturo);
                transactionScope.Complete();
            }

            return mensajeGenericoES.exito_RegistrarElemento;
        }
        public string RemoveInstrumentoFuturo(int idInstrumento, int idFuturo)
        {

            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);
            InstrumentoFuturo persistedFuturo = iInstrumentoFuturoRepository.Get(idFuturo);

            if (persisted == null || persistedFuturo == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_Futuro_Tiene_Dependencias));

            iInstrumentoFuturoRepository.Remove(persistedFuturo);
            iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
            iInstrumentoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_EliminarElemento;
        }
        public string AnnulInstrumentoFuturo(InstrumentoFuturoDTO instrumentoFuturoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFuturoDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoFuturoDTO.IdInstrumento);
                InstrumentoFuturo persistedFuturo = iInstrumentoFuturoRepository.Get(instrumentoFuturoDTO.IdFuturo);

                if (persisted == null || persistedFuturo == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int idAnulado = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

                Instrumento current = new Instrumento(instrumentoFuturoDTO.ComentarioAnulacion,
                    instrumentoFuturoDTO.LoginActualizacion);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndActividad = idAnulado;
                persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedFuturo.LoginActualizacion = current.LoginActualizacion;
                persistedFuturo.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedFuturo.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoFuturoRepository.Merge(persistedFuturo, persistedFuturo);
                iInstrumentoFuturoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_AnularElemento;
        }
        public string ActiveInstrumentoFuturo(InstrumentoFuturoDTO instrumentoFuturoDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoFuturoDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoFuturoDTO.IdInstrumento);
                if (!iAnexoIRubroRepository.Any(p => p.AnexoIRubroDetalleInstrumento.Any(q => q.IdTipoInstrumento == persisted.TipoInstrumento.IdTipoInstrumento && q.IdEmisor == persisted.IdEmisor)))
                    throw new ApplicationException("No tiene detalle en el Anexo III");
                InstrumentoFuturo persistedFuturo = iInstrumentoFuturoRepository.Get(instrumentoFuturoDTO.IdFuturo);

                if (persisted == null || persistedFuturo == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoFuturoDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoFuturoDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;

                Instrumento current = new Instrumento(instrumentoFuturoDTO.ComentarioHabilitacion,
                    instrumentoFuturoDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedFuturo.LoginActualizacion = current.LoginActualizacion;
                persistedFuturo.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedFuturo.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoFuturoRepository.Merge(persistedFuturo, persistedFuturo);
                iInstrumentoFuturoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensaje;
        }
        public InstrumentoFuturoDTO[] GetAllActiveHabilitadoInstrumentoFuturo()
        {
            return iInstrumentoFuturoRepository.GetAll().ProjectedAsCollection<InstrumentoFuturoDTO>().ToArray();
        }
        #endregion

        #region InstrumentoOpcionAppService Members
        public InstrumentoOpcionPagedDTO GetFilteredDataOpcion(string codigoSbs, int indTipoSubyacente, int indSubyacente, int indTipoOpcion, int indTipoExpiracion, int indActividad, int indHabilitacion, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;

            InstrumentoOpcionPagedDTO response = iInstrumentoDataRepository.GetFilteredDataOpcion(codigoSbs, indTipoSubyacente, indSubyacente, indTipoOpcion, indTipoExpiracion, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);
            foreach (InstrumentoOpcionListadoDTO item in response.ListaInstrumentoOpcion)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }
            return response;
        }
        public string UpdateInstrumentoOpcion(InstrumentoOpcionDTO instrumentoOpcionDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoOpcionDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoOpcion"));

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoOpcionDTO.IdInstrumento);
                InstrumentoOpcion persistedOpcion = iInstrumentoOpcionRepository.Get(instrumentoOpcionDTO.IdOpcion);

                if (persisted == null || persistedOpcion == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                VerifyNombreInstrumentoIsUnique(instrumentoOpcionDTO.CodigoSbs, instrumentoOpcionDTO.IdInstrumento);
                VerifyInstrumentoOpcionIsUnique(instrumentoOpcionDTO);

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoOpcionDTO.IndHabilitacionRiesgo);
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoOpcionDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                int IdSecuencialFechaEmision = (instrumentoOpcionDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoOpcionDTO.SecuencialFechaEmision);
                int IdSecuencialFechaFirstExercise = (instrumentoOpcionDTO.SecuencialFechaFirstExercise.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoOpcionDTO.SecuencialFechaFirstExercise);
                int IdSecuencialFechaExpiracion = (instrumentoOpcionDTO.SecuencialFechaExpiracion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoOpcionDTO.SecuencialFechaExpiracion);

                Instrumento current = new Instrumento(instrumentoOpcionDTO.IdTipoInstrumento, instrumentoOpcionDTO.NombreInstrumento, instrumentoOpcionDTO.CodigoSbs,
                    instrumentoOpcionDTO.IdMoneda, instrumentoOpcionDTO.IdEmisor, IdGrupoInstrumento, instrumentoOpcionDTO.IndCategoria,
                    instrumentoOpcionDTO.IndFamilia, indActividad, instrumentoOpcionDTO.IdClasificacionRiesgo, instrumentoOpcionDTO.LoginActualizacion,
                    indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, current);
                iInstrumentoRepository.UnitOfWork.Commit();

                InstrumentoOpcion instrumentoOpcion = new InstrumentoOpcion(instrumentoOpcionDTO.IdInstrumento, instrumentoOpcionDTO.IndTipoSubyacente,
                    instrumentoOpcionDTO.IndTipoOpcion, instrumentoOpcionDTO.IndSubyacente, instrumentoOpcionDTO.IndTipoExpiracion, instrumentoOpcionDTO.IdMonedaLiquidacion,
                    IdSecuencialFechaEmision, instrumentoOpcionDTO.Ticker, IdSecuencialFechaFirstExercise, instrumentoOpcionDTO.Strike,
                    IdSecuencialFechaExpiracion, instrumentoOpcionDTO.TamanioContrato, instrumentoOpcionDTO.ContratosCirculacion,
                    instrumentoOpcionDTO.TickSize, instrumentoOpcionDTO.ConversionFactor, instrumentoOpcionDTO.IndUnidades, instrumentoOpcionDTO.CUSIP,
                    instrumentoOpcionDTO.TieneMandato, instrumentoOpcionDTO.LoginActualizacion);

                instrumentoOpcion.IdOpcion = persistedOpcion.IdOpcion;
                iInstrumentoOpcionRepository.Merge(persistedOpcion, instrumentoOpcion);
                iInstrumentoOpcionRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_ActualizarElemento;
        }
        public InstrumentoOpcionDTO GetByIdInstrumentoOpcion(int idInstrumento, int idOpcion)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.Opcion = iInstrumentoOpcionRepository.Get(idOpcion);

            if (instrumento.Opcion == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string SecuencialFechaEmision = Helper.ConvertIdFechaToFechaString(instrumento.Opcion.IdSecuencialFechaEmision);
            string SecuencialFechaFirstExercise = Helper.ConvertIdFechaToFechaString(instrumento.Opcion.IdSecuencialFechaFirstExercise);
            string SecuencialFechaExpiracion = Helper.ConvertIdFechaToFechaString(instrumento.Opcion.IdSecuencialFechaExpiracion);

            instrumento.Opcion.SecuencialFechaEmision = SecuencialFechaEmision;
            instrumento.Opcion.SecuencialFechaFirstExercise = SecuencialFechaFirstExercise;
            instrumento.Opcion.SecuencialFechaExpiracion = SecuencialFechaExpiracion;

            return instrumento.ProjectedAs<InstrumentoOpcionDTO>();
        }
        public string AddNewInstrumentoOpcion(InstrumentoOpcionDTO instrumentoOpcionDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoOpcionDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);

                VerifyNombreInstrumentoIsUnique(instrumentoOpcionDTO.CodigoSbs, instrumentoOpcionDTO.IdInstrumento);
                VerifyInstrumentoOpcionIsUnique(instrumentoOpcionDTO);
                TipoInstrumentoDTO tipoInstrumentoDTO = iTipoInstrumentoAppService.GetById(instrumentoOpcionDTO.IdTipoInstrumento.Value);
                int IdGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoOpcionDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;
                int IdSecuencialFechaEmision = (instrumentoOpcionDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoOpcionDTO.SecuencialFechaEmision);
                int IdSecuencialFechaFirstExercise = (instrumentoOpcionDTO.SecuencialFechaFirstExercise.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoOpcionDTO.SecuencialFechaFirstExercise);
                int IdSecuencialFechaExpiracion = (instrumentoOpcionDTO.SecuencialFechaExpiracion.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoOpcionDTO.SecuencialFechaExpiracion);
                instrumentoOpcionDTO.NombreInstrumento = string.Format("{0} - {1}", tipoInstrumentoDTO.NombreSbsTipoInstrumento, instrumentoOpcionDTO.NombreInstrumento);
                Instrumento instrumento = new Instrumento(instrumentoOpcionDTO.IdTipoInstrumento, instrumentoOpcionDTO.NombreInstrumento, instrumentoOpcionDTO.CodigoSbs,
                    instrumentoOpcionDTO.IdMoneda, instrumentoOpcionDTO.IdEmisor, IdGrupoInstrumento, instrumentoOpcionDTO.IndCategoria,
                    instrumentoOpcionDTO.IndFamilia, indActividad, instrumentoOpcionDTO.IdClasificacionRiesgo, instrumentoOpcionDTO.LoginActualizacion,
                    indHabilitacionRiesgo);
                SaveInstrumento(instrumento);

                InstrumentoOpcion instrumentoOpcion = new InstrumentoOpcion(instrumento.IdInstrumento, instrumentoOpcionDTO.IndTipoSubyacente,
                    instrumentoOpcionDTO.IndTipoOpcion, instrumentoOpcionDTO.IndSubyacente, instrumentoOpcionDTO.IndTipoExpiracion, instrumentoOpcionDTO.IdMonedaLiquidacion,
                    IdSecuencialFechaEmision, instrumentoOpcionDTO.Ticker, IdSecuencialFechaFirstExercise, instrumentoOpcionDTO.Strike,
                    IdSecuencialFechaExpiracion, instrumentoOpcionDTO.TamanioContrato, instrumentoOpcionDTO.ContratosCirculacion,
                    instrumentoOpcionDTO.TickSize, instrumentoOpcionDTO.ConversionFactor, instrumentoOpcionDTO.IndUnidades, instrumentoOpcionDTO.CUSIP,
                    instrumentoOpcionDTO.TieneMandato, instrumentoOpcionDTO.LoginActualizacion);

                SaveInstrumentoOpcion(instrumentoOpcion);
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_RegistrarElemento;
        }
        public string RemoveInstrumentoOpcion(int idInstrumento, int idOpcion)
        {
            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);
            InstrumentoOpcion persistedOpcion = iInstrumentoOpcionRepository.Get(idOpcion);

            if (persisted == null || persistedOpcion == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_Opcion_Tiene_Dependencias));

            iInstrumentoOpcionRepository.Remove(persistedOpcion);
            iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
            iInstrumentoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_EliminarElemento;
        }
        public string AnnulInstrumentoOpcion(InstrumentoOpcionDTO instrumentoOpcionDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoOpcionDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoOpcionDTO.IdInstrumento);
                InstrumentoOpcion persistedOpcion = iInstrumentoOpcionRepository.Get(instrumentoOpcionDTO.IdOpcion);

                if (persisted == null || persistedOpcion == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int idAnulado = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

                Instrumento current = new Instrumento(instrumentoOpcionDTO.ComentarioAnulacion,
                    instrumentoOpcionDTO.LoginActualizacion);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndActividad = idAnulado;
                persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedOpcion.LoginActualizacion = current.LoginActualizacion;
                persistedOpcion.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedOpcion.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoOpcionRepository.Merge(persistedOpcion, persistedOpcion);
                iInstrumentoOpcionRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_AnularElemento;
        }
        public string ActiveInstrumentoOpcion(InstrumentoOpcionDTO instrumentoOpcionDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoOpcionDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoOpcionDTO.IdInstrumento);
                if (!iAnexoIRubroRepository.Any(p => p.AnexoIRubroDetalleInstrumento.Any(q => q.IdTipoInstrumento == persisted.TipoInstrumento.IdTipoInstrumento && q.IdEmisor == persisted.IdEmisor)))
                    throw new ApplicationException("No tiene detalle en el Anexo III");
                InstrumentoOpcion persistedOpcion = iInstrumentoOpcionRepository.Get(instrumentoOpcionDTO.IdOpcion);

                if (persisted == null || persistedOpcion == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoOpcionDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoOpcionDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;
                Instrumento current = new Instrumento(instrumentoOpcionDTO.ComentarioHabilitacion,
                    instrumentoOpcionDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedOpcion.LoginActualizacion = current.LoginActualizacion;
                persistedOpcion.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedOpcion.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoOpcionRepository.Merge(persistedOpcion, persistedOpcion);
                iInstrumentoOpcionRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensaje;
        }
        public InstrumentoOpcionDTO[] GetAllActiveHabilitadoInstrumentoOpcion()
        {
            var instrumentoOpciones = iInstrumentoOpcionRepository.GetAll().ProjectedAsCollection<InstrumentoOpcionDTO>().ToArray();

            foreach (var instrumentoOpcion in instrumentoOpciones)
            {
                instrumentoOpcion.FechaExpiracion = Helper.ConvertIdFechaToFechaString(instrumentoOpcion.IdSecuencialFechaExpiracion);
                instrumentoOpcion.EmisorSubyacente = iEntidadRepository.Get(instrumentoOpcion.IdEmisorSubyacente).NombreEntidad;
            }

            return instrumentoOpciones;
        }
        #endregion

        #region InstrumentoVectorPrecioSbsAppService Members
        public InstrumentoVectorPrecioSbsPagedDTO GetFilteredDataConsultaVectorPrecioCargado(string fecRefDel, string fecRefAl, string nemotecnico, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            nemotecnico = nemotecnico.Equals("_") ? "" : nemotecnico;
            columnName = columnName.Equals("_") ? "" : columnName;

            int IdfecRefDel = (fecRefDel.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fecRefDel);
            int IdfecRefAlo = (fecRefAl.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fecRefAl);

            InstrumentoVectorPrecioSbsPagedDTO response = iInstrumentoDataRepository.GetFilteredDataConsultaVectorPrecioCargado(IdfecRefDel, IdfecRefAlo, nemotecnico, currentIndexPage, itemsPerPage, columnName, isAscending);
            foreach (InstrumentoVectorPrecioSbsListadoDTO item in response.ListaInstrumentoVectorPrecioSbs)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();
                    if (lista[i] != string.Empty)
                    {
                        if (Convert.ToInt32(lista[i]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(lista[i])
                        });
                    }
                }
            }
            return response;
        }

        public ListadoGeneralDTO[] GetNemotecnicoOfInstrumentosByNemotecnico(string nemotecnico)
        {
            nemotecnico = nemotecnico.Equals("_") ? "" : nemotecnico;


            ListadoGeneralDTO[] response = iInstrumentoDataRepository.GetNemotecnicoOfInstrumentosByNemotecnico(nemotecnico);

            return response;
        }


        public InstrumentoVectorPrecioSbsListadoConsultaVectorVariacionDTO[] GetAllListadoInstrumentoPrecioForListaConsultaVectorVariacion(InstrumentoVectorPrecioSbsFilterConsultaVectorVariacionDTO filter)
        {
            if (string.IsNullOrEmpty(filter.SecuencialFechaIDI))
                throw new Exception(mensajeGenericoES.MSJ_002_IDI_Fecha_IDI_Obligatoria);

            filter.IdFechaIDI = Helper.ConvertFechaStringToIdFecha(filter.SecuencialFechaIDI);
            return iInstrumentoDataRepository.GetAllListadoInstrumentoPrecioForListaConsultaVectorVariacion(filter);
        }

        #endregion

        #region InstrumentoVectorPrecioCargaSbsAppService Members
        public InstrumentoVectorPrecioCargaSbsPagedDTO GetFilteredDataCargaVectorPrecioSbs(int idLote, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;


            InstrumentoVectorPrecioCargaSbsPagedDTO response = iInstrumentoDataRepository.GetFilteredDataCargaVectorPrecioSbs(idLote, currentIndexPage, itemsPerPage, columnName, isAscending);
            return response;
        }
        public bool ValidateIsCargadoInstrumentoVectorPrecio(int idLote)
        {
            var dataLote = iInstrumentoVectorPrecioCargaSbsRepository.FirstOrDefault(x => x.IdLote == idLote);
            if (dataLote != null)
            {
                var IdFecha = dataLote.IdFechaInstrumentoPrecioCarga;
                //var CodigosSbs = dataLote.Select(x => x.CodigoSbs).Distinct().ToArray();
                return iInstrumentoVectorPrecioSbsRepository.Any(x => x.IdFechaInstrumentoPrecio == IdFecha/* && CodigosSbs.Contains(x.Instrumento.CodigoSbs)*/);
            }
            return false;
        }
        public bool ValidateProcesoFondoAlternativo(int idFecha)
        {
            return iFondoAlternativoActualizacionPrecioRepository.Any(x => x.IdSecuencialFechaActualizacionPrecio == idFecha);
        }

        public ListadoGeneralDTO[] GetLatestVectorPrecioSbsLoaded()
        {
            ListadoGeneralDTO[] response = iInstrumentoDataRepository.GetLatestVectorPrecioSbsLoaded();
            return response;
        }

        #endregion

        #region InstrumentoCertificadoDepositoCortoPlazoAppService Members

        public InstrumentoCertificadoDepositoCortoPlazoPagedDTO GetFilteredDataCertificadoDepositoCortoPlazo(string codigoSbs, int idGrupoInstrumento, int idTipoInstrumento, int idEmisor, int idMoneda, string fechaEmision, string fechaVenc, int indActividad, int indHabilitacion, int currentIndexPage, int itemsPerPage, string columnName, bool isAscending)
        {
            currentIndexPage = currentIndexPage < 1 ? 1 : currentIndexPage;
            itemsPerPage = itemsPerPage < 0 ? 1 : itemsPerPage;
            columnName = columnName.Equals("_") ? "" : columnName;
            codigoSbs = codigoSbs.Equals("_") ? "" : codigoSbs;
            int idFechaEmision = (fechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fechaEmision);
            int idFechaVenc = (fechaVenc.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(fechaVenc);
            InstrumentoCertificadoDepositoCortoPlazoPagedDTO response = iInstrumentoDataRepository.GetFilteredDataCertificadoDepositoCortoPlazo(codigoSbs, idGrupoInstrumento, idTipoInstrumento, idEmisor, idMoneda, idFechaEmision, idFechaVenc, indActividad, indHabilitacion, currentIndexPage, itemsPerPage, columnName, isAscending);
            foreach (InstrumentoCertificadoDepositoCortoPlazoListadoDTO item in response.ListaInstrumentoCertificadoDepositoCortoPlazo)
            {
                string[] lista = item.Elementos.Split(',');
                for (int i = 0; i < lista.Count(); i++)
                {
                    if (item.Recorrido == null)
                        item.Recorrido = new List<PosicionDTO>();

                    string[] listaId = lista[i].Split('-');
                    if (listaId[0] != string.Empty)
                    {
                        if (Convert.ToInt32(listaId[0]) == item.IdInstrumento)
                        {
                            item.Posicion = i + 1;
                        }
                        item.Recorrido.Add(new PosicionDTO
                        {
                            Item = i + 1,
                            IdItem = Convert.ToInt32(listaId[0]),
                            IdSubItem = Convert.ToInt32(listaId[1])
                        });
                    }
                }
            }
            return response;
        }

        public string UpdateInstrumentoCertificadoDepositoCortoPlazo(InstrumentoCertificadoDepositoCortoPlazoDTO instrumentoCertificadoDepositoCortoPlazoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoCertificadoDepositoCortoPlazoDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoCertificadoDeposito"));

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoCertificadoDepositoCortoPlazoDTO.IdInstrumento);
                InstrumentoCertificadoDepositoCortoPlazo persistedCertificadoDeposito = iInstrumentoCertificadoDepositoCortoPlazoRepository.Get(instrumentoCertificadoDepositoCortoPlazoDTO.IdCertificadoDepositoCortoPlazo);

                if (persisted == null || persistedCertificadoDeposito == null)
                    throw new Exception(mensajeGenericoES.error_ActualizarElementoYaNoExiste);

                VerifyNombreInstrumentoIsUnique(instrumentoCertificadoDepositoCortoPlazoDTO.CodigoSbs, instrumentoCertificadoDepositoCortoPlazoDTO.IdInstrumento);
                VerifyInstrumentoCertificadoDepositoIsUnique(instrumentoCertificadoDepositoCortoPlazoDTO);

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indEstadoVigenciaCuponVigente = iIndicadorAppService.GetId((int)eIndicador.EstadoVigenciaCupon, (int)eEstadoVigenciaCupon.Vigente);
                int indEstadoVigenciaCuponVencido = iIndicadorAppService.GetId((int)eIndicador.EstadoVigenciaCupon, (int)eEstadoVigenciaCupon.Vencido);
                int indHabilitacionRiesgo = iIndicadorAppService.GetNewIdEnableRiskByUpdate(instrumentoCertificadoDepositoCortoPlazoDTO.IndHabilitacionRiesgo);
                int idGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoCertificadoDepositoCortoPlazoDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;

                int IdSecuencialFechaEmision = (instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaEmision);
                int IdSecuencialFechaVencimiento = (instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaVencimiento.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaVencimiento);
                int IdSecuencialFechaMontoEmitido = (instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaMontoEmitido.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaMontoEmitido);

                //IndicadorDTO indicadorInteres = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.IndicadorInteresesCertificadoDeposito, instrumentoCertificadoDepositoCortoPlazoDTO.IndTipoInteres);
                //if (indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.CeroCupon))
                //    instrumentoCertificadoDepositoCortoPlazoDTO.Cuponera = GenerarCeroCupon(instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaEmision, instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaVencimiento, instrumentoCertificadoDepositoCortoPlazoDTO.ValorNominal, instrumentoCertificadoDepositoCortoPlazoDTO.IdMercado);

                Instrumento current = new Instrumento(instrumentoCertificadoDepositoCortoPlazoDTO.IdTipoInstrumento, instrumentoCertificadoDepositoCortoPlazoDTO.NombreInstrumento, instrumentoCertificadoDepositoCortoPlazoDTO.CodigoSbs,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IdMoneda, instrumentoCertificadoDepositoCortoPlazoDTO.IdEmisor, idGrupoInstrumento, instrumentoCertificadoDepositoCortoPlazoDTO.IndCategoria,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IndFamilia, indActividad, instrumentoCertificadoDepositoCortoPlazoDTO.IdClasificacionRiesgo, instrumentoCertificadoDepositoCortoPlazoDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, current);
                iInstrumentoRepository.UnitOfWork.Commit();
                InstrumentoCertificadoDepositoCortoPlazo instrumentoCertificadosDepositos = new InstrumentoCertificadoDepositoCortoPlazo(instrumentoCertificadoDepositoCortoPlazoDTO.IdInstrumento,
                    instrumentoCertificadoDepositoCortoPlazoDTO.Nemotecnico, instrumentoCertificadoDepositoCortoPlazoDTO.CodigoIsin, instrumentoCertificadoDepositoCortoPlazoDTO.TieneMonedaDual,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IdMonedaDual, instrumentoCertificadoDepositoCortoPlazoDTO.CodigoCavali, instrumentoCertificadoDepositoCortoPlazoDTO.ValorNominal,
                    instrumentoCertificadoDepositoCortoPlazoDTO.ValorNominalSbs, IdSecuencialFechaEmision, IdSecuencialFechaVencimiento,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IndTipoCustodia, instrumentoCertificadoDepositoCortoPlazoDTO.IndUnidad, instrumentoCertificadoDepositoCortoPlazoDTO.IndValorizacion,
                    instrumentoCertificadoDepositoCortoPlazoDTO.MontoEmitido, IdSecuencialFechaMontoEmitido, instrumentoCertificadoDepositoCortoPlazoDTO.MontoColocado,
                    instrumentoCertificadoDepositoCortoPlazoDTO.PlazoDias, instrumentoCertificadoDepositoCortoPlazoDTO.IndGarantia, instrumentoCertificadoDepositoCortoPlazoDTO.IndTipoInteres,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IdFiadorAvalador, instrumentoCertificadoDepositoCortoPlazoDTO.TieneMandato, instrumentoCertificadoDepositoCortoPlazoDTO.IndClasificadora1,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IndRatingClasificacion1, instrumentoCertificadoDepositoCortoPlazoDTO.IndClasificadora2, instrumentoCertificadoDepositoCortoPlazoDTO.IndRatingClasificacion2,
                    instrumentoCertificadoDepositoCortoPlazoDTO.Emision, instrumentoCertificadoDepositoCortoPlazoDTO.Programa, instrumentoCertificadoDepositoCortoPlazoDTO.Serie,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IndRegionEmision, instrumentoCertificadoDepositoCortoPlazoDTO.IndPaisEmision, instrumentoCertificadoDepositoCortoPlazoDTO.IndFocoGeograficoEmision,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IndClase, instrumentoCertificadoDepositoCortoPlazoDTO.IndTipoAmortizacion, instrumentoCertificadoDepositoCortoPlazoDTO.IndPeriodoPago,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IndBaseCalculo, instrumentoCertificadoDepositoCortoPlazoDTO.NroCupones, instrumentoCertificadoDepositoCortoPlazoDTO.IndIndexarInflacion,
                    instrumentoCertificadoDepositoCortoPlazoDTO.PorcentajeTasaFija, instrumentoCertificadoDepositoCortoPlazoDTO.IndTasaFlotante, instrumentoCertificadoDepositoCortoPlazoDTO.PorcentajeTasaFlotante,
                    instrumentoCertificadoDepositoCortoPlazoDTO.Observaciones, instrumentoCertificadoDepositoCortoPlazoDTO.LoginActualizacion, instrumentoCertificadoDepositoCortoPlazoDTO.IdPlantillaDepositoPlazo, instrumentoCertificadoDepositoCortoPlazoDTO.IdMercado, instrumentoCertificadoDepositoCortoPlazoDTO.IdTipoTasa);
                instrumentoCertificadosDepositos.IdCertificadoDepositoCortoPlazo = persistedCertificadoDeposito.IdCertificadoDepositoCortoPlazo;
                iInstrumentoCertificadoDepositoCortoPlazoRepository.Merge(persistedCertificadoDeposito, instrumentoCertificadosDepositos);
                iInstrumentoCertificadoDepositoCortoPlazoRepository.UnitOfWork.Commit();

                List<InstrumentoCertificadoDepositoCortoPlazoCupon> cuponesRegistrados = iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.GetFiltered(irfc => irfc.IdCertificadoDepositoCortoPlazo.Equals(persistedCertificadoDeposito.IdCertificadoDepositoCortoPlazo)).ToList();

                if (cuponesRegistrados.Any(x => x.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido ||
                                                x.VencimientoCertificadoDepositoCortoPlazoCupon.Any(z => z.IndEstadoVencimiento == indEstadoVigenciaCuponVencido)))
                {
                    var cuponesVigentes = cuponesRegistrados.FindAll(cr => cr.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVigente && !cr.VencimientoCertificadoDepositoCortoPlazoCupon.Any()).ToArray();
                    foreach (var cupon in cuponesVigentes)
                        iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.Remove(cupon);

                    iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.UnitOfWork.Commit();

                    var cuponesVencidos = cuponesRegistrados.FindAll(cr => cr.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido || cr.VencimientoCertificadoDepositoCortoPlazoCupon.Any()).ToArray();

                    foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoCertificadoDepositoCortoPlazoDTO.Cuponera)
                        if (!cuponesVencidos.Any(x => x.NumeroCupon == cupon.NumeroCupon))
                            AddNewCertificadoDepositoCupon(cupon, instrumentoCertificadosDepositos.IdCertificadoDepositoCortoPlazo, instrumentoCertificadosDepositos.LoginActualizacion);
                }
                else
                {
                    foreach (var cupon in cuponesRegistrados)
                        iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.Remove(cupon);

                    iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();
                    foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoCertificadoDepositoCortoPlazoDTO.Cuponera)
                        AddNewCertificadoDepositoCupon(cupon, instrumentoCertificadosDepositos.IdCertificadoDepositoCortoPlazo, instrumentoCertificadosDepositos.LoginActualizacion);

                    foreach (var item in persisted.VariacionValorNominalVigente.ToArray())
                        persisted.VariacionValorNominalVigente.Remove(item);

                    var primerCuponVigente = iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.GetFiltered(irfc => irfc.IdCertificadoDepositoCortoPlazo.Equals(instrumentoCertificadosDepositos.IdCertificadoDepositoCortoPlazo) &&
                                                                                                  irfc.IndEstadoVigenciaCupon != indEstadoVigenciaCuponVencido)
                                                                             .OrderBy(x => x.FechaCorte)
                                                                             .FirstOrDefault();

                    persisted.VariacionValorNominalVigente.Add(new VariacionValorNominalVigente
                    {
                        IdSecuencialFecha = Helper.ConvertToIdFecha(primerCuponVigente.FechaInicio),
                        ValorNominalVigente = primerCuponVigente.SaldoAmortizacion
                    });
                }

                if (!instrumentoCertificadoDepositoCortoPlazoDTO.Cuponera.Any(x => x.IdIndicador == (int)eEstadoVigenciaCupon.Vigente))
                    persisted.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vencido);
                else
                    persisted.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);

                var ultimoCupon = instrumentoCertificadoDepositoCortoPlazoDTO.Cuponera.OrderByDescending(x => Helper.ConvertFechaStringToIdFecha(x.FechaCorte)).FirstOrDefault();
                if (ultimoCupon != null)
                    persistedCertificadoDeposito.IdSecuencialFechaVencimiento = Helper.ConvertFechaStringToIdFecha(ultimoCupon.FechaCorte);


                var cuponesVencidosGuardados = iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.GetFiltered(irfc => irfc.IdCertificadoDepositoCortoPlazo == persistedCertificadoDeposito.IdCertificadoDepositoCortoPlazo &&
                                                                                                   irfc.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido).ToList();

                persistedCertificadoDeposito.ValorNominalVigente = persistedCertificadoDeposito.ValorNominal;
                if (cuponesVencidosGuardados.Any())
                {
                    var amortisacion = cuponesVencidosGuardados.Sum(x => x.ImporteAmortizacion);
                    persistedCertificadoDeposito.ValorNominalVigente = persistedCertificadoDeposito.ValorNominal - amortisacion;
                }

                iIndicadorRepository.UnitOfWork.Commit();

                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_ActualizarElemento;
        }

        public InstrumentoCertificadoDepositoCortoPlazoDTO GetByIdInstrumentoCertificadoDepositoCortoPlazo(int idInstrumento, int idCertificadoDepositoCortoPlazo)
        {
            if (idInstrumento == 0)
                throw new ArgumentException(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento instrumento = iInstrumentoRepository.Get(idInstrumento);

            if (instrumento == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            instrumento.CertificadoDepositoCortoPlazo = iInstrumentoCertificadoDepositoCortoPlazoRepository.Get(idCertificadoDepositoCortoPlazo);

            if (instrumento.CertificadoDepositoCortoPlazo == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            string FechaEmision = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoDepositoCortoPlazo.IdSecuencialFechaEmision);
            string FechaMontoEmitido = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoDepositoCortoPlazo.IdSecuencialFechaMontoEmitido);
            string FechaVencimiento = Helper.ConvertIdFechaToFechaString(instrumento.CertificadoDepositoCortoPlazo.IdSecuencialFechaVencimiento);

            instrumento.CertificadoDepositoCortoPlazo.SecuencialFechaEmision = FechaEmision;
            instrumento.CertificadoDepositoCortoPlazo.SecuencialFechaMontoEmitido = FechaMontoEmitido;
            instrumento.CertificadoDepositoCortoPlazo.SecuencialFechaVencimiento = FechaVencimiento;

            InstrumentoCertificadoDepositoCortoPlazoDTO instrumentoCertificadoDeposito = instrumento.ProjectedAs<InstrumentoCertificadoDepositoCortoPlazoDTO>();
            instrumentoCertificadoDeposito.Cuponera = instrumento.CertificadoDepositoCortoPlazo.CertificadoDepositoCortoPlazoCupon.ProjectedAsCollection<InstrumentoCuponeraListadoDTO>().ToArray();
            List<IndicadorDTO> indicadoresEstadoVigenciaCupon = iIndicadorAppService.GetAllByTipoIndicadorAndActive((int)eIndicador.EstadoVigenciaCupon);
            List<IndicadorDTO> indicadoresEstadoCupon = iIndicadorAppService.GetAllByTipoIndicadorAndActive((int)eIndicador.EstadoCupon);
            for (int i = 0; i < instrumentoCertificadoDeposito.Cuponera.Length; i++)
            {
                IndicadorDTO indicadorVigenciaCupon = indicadoresEstadoVigenciaCupon.Find(ic => ic.Id.Equals(instrumentoCertificadoDeposito.Cuponera[i].IdIndicador));
                IndicadorDTO indicadorCupon = indicadoresEstadoCupon.Find(ic => ic.Id.Equals(instrumentoCertificadoDeposito.Cuponera[i].IdIndicadorEstadoCupon));
                instrumentoCertificadoDeposito.Cuponera[i].FechaInicio = instrumentoCertificadoDeposito.Cuponera[i].FechaInicioDate.ToString("dd/MM/yyyy");
                instrumentoCertificadoDeposito.Cuponera[i].FechaCorte = instrumentoCertificadoDeposito.Cuponera[i].FechaCorteDate.ToString("dd/MM/yyyy");
                instrumentoCertificadoDeposito.Cuponera[i].FechaPago = instrumentoCertificadoDeposito.Cuponera[i].FechaPagoDate.ToString("dd/MM/yyyy");
                instrumentoCertificadoDeposito.Cuponera[i].Indicador = indicadorVigenciaCupon.Descripcion;
                instrumentoCertificadoDeposito.Cuponera[i].IdIndicador = indicadorVigenciaCupon.IdIndicador;
                instrumentoCertificadoDeposito.Cuponera[i].EstadoCupon = indicadorCupon.Descripcion;
                instrumentoCertificadoDeposito.Cuponera[i].IdIndicadorEstadoCupon = indicadorCupon.IdIndicador;
            }
            return instrumentoCertificadoDeposito;
        }

        public string AddNewInstrumentoCertificadoDepositoCortoPlazo(InstrumentoCertificadoDepositoCortoPlazoDTO instrumentoCertificadoDepositoCortoPlazoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoCertificadoDepositoCortoPlazoDTO == null)
                    throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoDTO"));

                int indActividad = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Vigente);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoNuevo);
                int indEstadoVigenciaCuponVencido = iIndicadorAppService.GetId((int)eIndicador.EstadoVigenciaCupon, (int)eEstadoVigenciaCupon.Vencido);

                VerifyNombreInstrumentoIsUnique(instrumentoCertificadoDepositoCortoPlazoDTO.CodigoSbs, instrumentoCertificadoDepositoCortoPlazoDTO.IdInstrumento);
                VerifyInstrumentoCertificadoDepositoIsUnique(instrumentoCertificadoDepositoCortoPlazoDTO);
                MonedaDTO monedaIndexada = VerifyMonedaIndexada(instrumentoCertificadoDepositoCortoPlazoDTO.IndTipoInteres, instrumentoCertificadoDepositoCortoPlazoDTO.IdMoneda, false);
                if (monedaIndexada.IdMoneda > 0)
                {
                    StringBuilder codigoSBS = new StringBuilder(instrumentoCertificadoDepositoCortoPlazoDTO.CodigoSbs);
                    codigoSBS[6] = monedaIndexada.CodigoSBS[0];
                    instrumentoCertificadoDepositoCortoPlazoDTO.CodigoSbs = codigoSBS.ToString();
                }

                int idGrupoInstrumento = iTipoInstrumentoAppService.GetById(instrumentoCertificadoDepositoCortoPlazoDTO.IdTipoInstrumento.Value).IdGrupoInstrumento;

                int IdSecuencialFechaEmision = (instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaEmision.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaEmision);
                int IdSecuencialFechaVencimiento = (instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaVencimiento.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaVencimiento);
                int IdSecuencialFechaMontoEmitido = (instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaMontoEmitido.Equals("_")) ? 0 : Helper.ConvertFechaStringToIdFecha(instrumentoCertificadoDepositoCortoPlazoDTO.SecuencialFechaMontoEmitido);

                Instrumento instrumento = new Instrumento(instrumentoCertificadoDepositoCortoPlazoDTO.IdTipoInstrumento, instrumentoCertificadoDepositoCortoPlazoDTO.NombreInstrumento, instrumentoCertificadoDepositoCortoPlazoDTO.CodigoSbs,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IdMoneda, instrumentoCertificadoDepositoCortoPlazoDTO.IdEmisor, idGrupoInstrumento, instrumentoCertificadoDepositoCortoPlazoDTO.IndCategoria,
                    instrumentoCertificadoDepositoCortoPlazoDTO.IndFamilia, indActividad, instrumentoCertificadoDepositoCortoPlazoDTO.IdClasificacionRiesgo, instrumentoCertificadoDepositoCortoPlazoDTO.LoginActualizacion,
                    indHabilitacionRiesgo);

                SaveInstrumento(instrumento);

                InstrumentoCertificadoDepositoCortoPlazo instrumentoCertificadosDepositos = new InstrumentoCertificadoDepositoCortoPlazo(instrumento.IdInstrumento,
                   instrumentoCertificadoDepositoCortoPlazoDTO.Nemotecnico, instrumentoCertificadoDepositoCortoPlazoDTO.CodigoIsin, instrumentoCertificadoDepositoCortoPlazoDTO.TieneMonedaDual,
                   instrumentoCertificadoDepositoCortoPlazoDTO.IdMonedaDual, instrumentoCertificadoDepositoCortoPlazoDTO.CodigoCavali, instrumentoCertificadoDepositoCortoPlazoDTO.ValorNominal,
                   instrumentoCertificadoDepositoCortoPlazoDTO.ValorNominalSbs, IdSecuencialFechaEmision, IdSecuencialFechaVencimiento,
                   instrumentoCertificadoDepositoCortoPlazoDTO.IndTipoCustodia, instrumentoCertificadoDepositoCortoPlazoDTO.IndUnidad, instrumentoCertificadoDepositoCortoPlazoDTO.IndValorizacion,
                   instrumentoCertificadoDepositoCortoPlazoDTO.MontoEmitido, IdSecuencialFechaMontoEmitido, instrumentoCertificadoDepositoCortoPlazoDTO.MontoColocado,
                   instrumentoCertificadoDepositoCortoPlazoDTO.PlazoDias, instrumentoCertificadoDepositoCortoPlazoDTO.IndGarantia, instrumentoCertificadoDepositoCortoPlazoDTO.IndTipoInteres,
                   instrumentoCertificadoDepositoCortoPlazoDTO.IdFiadorAvalador, instrumentoCertificadoDepositoCortoPlazoDTO.TieneMandato, instrumentoCertificadoDepositoCortoPlazoDTO.IndClasificadora1,
                   instrumentoCertificadoDepositoCortoPlazoDTO.IndRatingClasificacion1, instrumentoCertificadoDepositoCortoPlazoDTO.IndClasificadora2, instrumentoCertificadoDepositoCortoPlazoDTO.IndRatingClasificacion2,
                   instrumentoCertificadoDepositoCortoPlazoDTO.Emision, instrumentoCertificadoDepositoCortoPlazoDTO.Programa, instrumentoCertificadoDepositoCortoPlazoDTO.Serie,
                   instrumentoCertificadoDepositoCortoPlazoDTO.IndRegionEmision, instrumentoCertificadoDepositoCortoPlazoDTO.IndPaisEmision, instrumentoCertificadoDepositoCortoPlazoDTO.IndFocoGeograficoEmision,
                   instrumentoCertificadoDepositoCortoPlazoDTO.IndClase, instrumentoCertificadoDepositoCortoPlazoDTO.IndTipoAmortizacion, instrumentoCertificadoDepositoCortoPlazoDTO.IndPeriodoPago,
                   instrumentoCertificadoDepositoCortoPlazoDTO.IndBaseCalculo, instrumentoCertificadoDepositoCortoPlazoDTO.NroCupones, instrumentoCertificadoDepositoCortoPlazoDTO.IndIndexarInflacion,
                   instrumentoCertificadoDepositoCortoPlazoDTO.PorcentajeTasaFija, instrumentoCertificadoDepositoCortoPlazoDTO.IndTasaFlotante, instrumentoCertificadoDepositoCortoPlazoDTO.PorcentajeTasaFlotante,
                   instrumentoCertificadoDepositoCortoPlazoDTO.Observaciones, instrumentoCertificadoDepositoCortoPlazoDTO.LoginActualizacion, instrumentoCertificadoDepositoCortoPlazoDTO.IdPlantillaDepositoPlazo, instrumentoCertificadoDepositoCortoPlazoDTO.IdMercado, instrumentoCertificadoDepositoCortoPlazoDTO.IdTipoTasa);

                SaveInstrumentoCertificadoDepositoCortoPlazo(instrumentoCertificadosDepositos);

                foreach (InstrumentoCuponeraListadoDTO cupon in instrumentoCertificadoDepositoCortoPlazoDTO.Cuponera)
                {
                    AddNewCertificadoDepositoCupon(cupon, instrumentoCertificadosDepositos.IdCertificadoDepositoCortoPlazo, instrumentoCertificadoDepositoCortoPlazoDTO.LoginActualizacion);
                }

                if (!instrumentoCertificadoDepositoCortoPlazoDTO.Cuponera.Any(x => x.IdIndicador == (int)eEstadoVigenciaCupon.Vigente))
                    instrumento.IndActividad = this.iIndicadorRepository.GetId((int)eIndicador.Estado, (int)eEstado.Vencido);

                var ultimoCupon = instrumentoCertificadoDepositoCortoPlazoDTO.Cuponera.OrderByDescending(x => Helper.ConvertFechaStringToIdFecha(x.FechaCorte)).FirstOrDefault();
                if (ultimoCupon != null)
                    instrumentoCertificadosDepositos.IdSecuencialFechaVencimiento = Helper.ConvertFechaStringToIdFecha(ultimoCupon.FechaCorte);


                var cuponesVencidosGuardados = iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.GetFiltered(irfc => irfc.IdCertificadoDepositoCortoPlazo == instrumentoCertificadosDepositos.IdCertificadoDepositoCortoPlazo &&
                                                                                                   irfc.IndEstadoVigenciaCupon == indEstadoVigenciaCuponVencido).ToList();

                instrumentoCertificadosDepositos.ValorNominalVigente = instrumentoCertificadosDepositos.ValorNominal;
                if (cuponesVencidosGuardados.Any())
                {
                    var amortisacion = cuponesVencidosGuardados.Sum(x => x.ImporteAmortizacion);
                    instrumentoCertificadosDepositos.ValorNominalVigente = instrumentoCertificadosDepositos.ValorNominal - amortisacion;

                    var ultimoCuponVencido = cuponesVencidosGuardados.OrderByDescending(x => Helper.ConvertToIdFecha(x.FechaCorte)).FirstOrDefault();
                    instrumento.VariacionValorNominalVigente.Add(new VariacionValorNominalVigente
                    {
                        IdSecuencialFecha = Helper.ConvertToIdFecha(ultimoCuponVencido.FechaCorte),
                        ValorNominalVigente = instrumentoCertificadosDepositos.ValorNominalVigente.Value
                    });
                }
                else
                {
                    var primerCuponVigente = iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.GetFiltered(irfc => irfc.IdCertificadoDepositoCortoPlazo.Equals(instrumentoCertificadosDepositos.IdCertificadoDepositoCortoPlazo) &&
                                                                                                      irfc.IndEstadoVigenciaCupon != indEstadoVigenciaCuponVencido)
                                                                                 .OrderBy(x => x.FechaCorte)
                                                                                 .FirstOrDefault();
                    instrumento.VariacionValorNominalVigente.Add(new VariacionValorNominalVigente
                    {
                        IdSecuencialFecha = Helper.ConvertToIdFecha(primerCuponVigente.FechaInicio),
                        ValorNominalVigente = instrumentoCertificadosDepositos.ValorNominalVigente.Value
                    });
                }


                iIndicadorRepository.UnitOfWork.Commit();

                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_RegistrarElemento;
        }

        public string RemoveInstrumentoCertificadoDepositoCortoPlazo(int idInstrumento, int idCertificadoDepositoCortoPlazo)
        {

            if (idInstrumento == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            Instrumento persisted = iInstrumentoRepository.Get(idInstrumento);
            InstrumentoCertificadoDepositoCortoPlazo persistedCertificadoDeposito = iInstrumentoCertificadoDepositoCortoPlazoRepository.Get(idCertificadoDepositoCortoPlazo);
            List<InstrumentoCertificadoDepositoCortoPlazoCupon> persistedCertificadoDepositoCupon = iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.GetFiltered(obj => obj.IdCertificadoDepositoCortoPlazo.Equals(idCertificadoDepositoCortoPlazo)).ToList();

            //if (persisted == null || persistedCertificadoDeposito == null || persistedCertificadoDepositoCupon.Count() <= 0)
            //    throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            if (persisted == null || persistedCertificadoDeposito == null)
                throw new KeyNotFoundException(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            foreach (InstrumentoCertificadoDepositoCortoPlazoCupon instrumentoCertificadoDepositoCupon in persistedCertificadoDepositoCupon)
            {
                iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.Remove(instrumentoCertificadoDepositoCupon);
                iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.UnitOfWork.Commit();
            }

            bool hasExistingDependencies = iInstrumentoDataRepository.HasExistingDependencies(idInstrumento);
            if (!hasExistingDependencies)
                throw new ApplicationException(string.Format(mensajeGenericoES.error_EliminarElemento, mensajeGenericoES.MSJ_002_Instrumento_CertificadoDepositoCortoPlazo_Tiene_Dependencias));


            iInstrumentoCertificadoDepositoCortoPlazoRepository.Remove(persistedCertificadoDeposito);
            iInstrumentoRepository.RemoveInstrumentoOnCascade(persisted);
            iInstrumentoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_EliminarElemento;
        }

        public string AnnulInstrumentoCertificadoDepositoCortoPlazo(InstrumentoCertificadoDepositoCortoPlazoDTO instrumentoCertificadoDepositoCortoPlazoDTO)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoCertificadoDepositoCortoPlazoDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoCertificadoDepositoCortoPlazoDTO.IdInstrumento);
                InstrumentoCertificadoDepositoCortoPlazo persistedCertificadoDeposito = iInstrumentoCertificadoDepositoCortoPlazoRepository.Get(instrumentoCertificadoDepositoCortoPlazoDTO.IdCertificadoDepositoCortoPlazo);
                //List<InstrumentoCertificadoDepositoCupon> persistedCertificadoDepositoCupon = iInstrumentoCertificadoDepositoCuponRepository.GetFiltered(obj => obj.IdCertificadoDeposito.Equals(instrumentoCertificadoDepositoDTO.IdCertificadoDeposito)).ToList();

                int idAnulado = iIndicadorAppService.GetId((int)eIndicador.Estado, (int)eEstado.Anulado);
                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, (int)eTipoHabilitacion.InhabilitadoModificado);

                Instrumento current = new Instrumento(instrumentoCertificadoDepositoCortoPlazoDTO.ComentarioAnulacion,
                    instrumentoCertificadoDepositoCortoPlazoDTO.LoginActualizacion);
                current.IdInstrumento = persisted.IdInstrumento;


                persisted.IndActividad = idAnulado;
                persisted.ComentarioAnulacion = current.ComentarioAnulacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedCertificadoDeposito.LoginActualizacion = current.LoginActualizacion;
                persistedCertificadoDeposito.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedCertificadoDeposito.UsuarioActualizacion = current.UsuarioActualizacion;

                //foreach (InstrumentoCertificadoDepositoCupon instrumentoCertificadoDepositoCupon in persistedCertificadoDepositoCupon)
                //{
                //    instrumentoCertificadoDepositoCupon.LoginActualizacion = current.LoginActualizacion;
                //    instrumentoCertificadoDepositoCupon.FechaHoraActualizacion = current.FechaHoraActualizacion;
                //    instrumentoCertificadoDepositoCupon.UsuarioActualizacion = current.UsuarioActualizacion;
                //    iInstrumentoCertificadoDepositoCuponRepository.Merge(instrumentoCertificadoDepositoCupon, instrumentoCertificadoDepositoCupon);
                //    iInstrumentoCertificadoDepositoCuponRepository.UnitOfWork.Commit();
                //}


                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoCertificadoDepositoCortoPlazoRepository.Merge(persistedCertificadoDeposito, persistedCertificadoDeposito);
                iInstrumentoCertificadoDepositoCortoPlazoRepository.UnitOfWork.Commit();

                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensajeGenericoES.exito_AnularElemento;
        }

        public string ActiveInstrumentoCertificadoDepositoCortoPlazo(InstrumentoCertificadoDepositoCortoPlazoDTO instrumentoCertificadoDepositoCortoPlazoDTO)
        {
            string mensaje = string.Empty;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, transactionScopeOptions))
            {
                if (instrumentoCertificadoDepositoCortoPlazoDTO.IdInstrumento == 0)
                    throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

                Instrumento persisted = iInstrumentoRepository.Get(instrumentoCertificadoDepositoCortoPlazoDTO.IdInstrumento);
                if (!iAnexoIRubroRepository.Any(p => p.AnexoIRubroDetalleInstrumento.Any(q => q.IdTipoInstrumento == persisted.TipoInstrumento.IdTipoInstrumento && q.IdEmisor == persisted.IdEmisor)))
                    throw new ApplicationException("No tiene detalle en el Anexo III");
                InstrumentoCertificadoDepositoCortoPlazo persistedCertificadoDeposito = iInstrumentoCertificadoDepositoCortoPlazoRepository.Get(instrumentoCertificadoDepositoCortoPlazoDTO.IdCertificadoDepositoCortoPlazo);
                //List<InstrumentoCertificadoDepositoCupon> persistedCertificadoDepositoCupon = iInstrumentoCertificadoDepositoCuponRepository.GetFiltered(obj => obj.IdCertificadoDeposito.Equals(instrumentoCertificadoDepositoDTO.IdCertificadoDeposito)).ToList();

                int indHabilitacionRiesgo = iIndicadorAppService.GetId((int)eIndicador.TipoHabilitacion, instrumentoCertificadoDepositoCortoPlazoDTO.IndHabilitacionRiesgo);
                mensaje = instrumentoCertificadoDepositoCortoPlazoDTO.IndHabilitacionRiesgo == (int)eTipoHabilitacion.Habilitado ? mensajeGenericoES.exito_HabilitarElemento : mensajeGenericoES.exito_InhabilitarElemento;
                Instrumento current = new Instrumento(instrumentoCertificadoDepositoCortoPlazoDTO.ComentarioHabilitacion,
                    instrumentoCertificadoDepositoCortoPlazoDTO.LoginActualizacion, indHabilitacionRiesgo);
                current.IdInstrumento = persisted.IdInstrumento;

                persisted.IndHabilitacionRiesgo = current.IndHabilitacionRiesgo;
                persisted.ComentarioHabilitacion = current.ComentarioHabilitacion;
                persisted.LoginActualizacion = current.LoginActualizacion;
                persisted.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persisted.UsuarioActualizacion = current.UsuarioActualizacion;

                persistedCertificadoDeposito.LoginActualizacion = current.LoginActualizacion;
                persistedCertificadoDeposito.FechaHoraActualizacion = current.FechaHoraActualizacion;
                persistedCertificadoDeposito.UsuarioActualizacion = current.UsuarioActualizacion;

                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                iInstrumentoCertificadoDepositoCortoPlazoRepository.Merge(persistedCertificadoDeposito, persistedCertificadoDeposito);
                iInstrumentoCertificadoDepositoCortoPlazoRepository.UnitOfWork.Commit();

                current.IdInstrumento = persisted.IdInstrumento;
                iInstrumentoRepository.Merge(persisted, persisted);
                iInstrumentoRepository.UnitOfWork.Commit();
                transactionScope.Complete();
            }
            return mensaje;
        }
        #endregion

        #region FondoAlternativoTasa Members

        public string AddNewFondoAlternativoTasa(InstrumentoFondoAlternativoTasaDTO fondoAlternativoTasaDTO,
            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (fondoAlternativoTasaDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoTasaDTO"));

            if (instrumentoFondoAlternativoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoDTO"));

            decimal MaxDecimal = (decimal)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXDecimal).ValorAuxDecimal1;
            //MAX
            decimal TasaHasta = (fondoAlternativoTasaDTO.TasaHasta.Equals(-1)) ? MaxDecimal : fondoAlternativoTasaDTO.TasaHasta;

            if (instrumentoFondoAlternativoDTO.TasaList != null)
            {
                foreach (var tasaList in instrumentoFondoAlternativoDTO.TasaList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.Descripcion.ToLower() == tasaList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = i + 1;
                    tasaList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).Descripcion;
                    UpdateFondoAlternativoTasa(tasaList, instrumentoFondoAlternativoDTO, loginActualizacion, fechaHoraActualizacion);
                }
            }

            InstrumentoFondoAlternativoTasa fondoAlternativoTasa = new InstrumentoFondoAlternativoTasa(fondoAlternativoTasaDTO.IdFondoAlternativo,
                fondoAlternativoTasaDTO.Seccion, TasaHasta, fondoAlternativoTasaDTO.TasaValor,
                loginActualizacion, fechaHoraActualizacion);

            SaveFondoAlternativoTasa(fondoAlternativoTasa);

            return mensajeGenericoES.exito_RegistrarElemento;
        }

        public string UpdateFondoAlternativoTasa(InstrumentoFondoAlternativoTasaDTO instrumentofondoAlternativoTasaDTO,
            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (instrumentofondoAlternativoTasaDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentofondoAlternativoTasaDTO"));

            if (instrumentoFondoAlternativoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoDTO"));

            decimal MaxDecimal = (decimal)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXDecimal).ValorAuxDecimal1;
            //MAX                            
            decimal TasaHasta = (instrumentofondoAlternativoTasaDTO.TasaHasta.Equals(-1)) ? MaxDecimal : instrumentofondoAlternativoTasaDTO.TasaHasta;

            InstrumentoFondoAlternativoTasa persistedTasa = iFondoAlternativoTasaRepository.Get(instrumentofondoAlternativoTasaDTO.IdFondoAlternativoTasa);
            InstrumentoFondoAlternativoTasa instrumentoFondoAlternativoTasa = new InstrumentoFondoAlternativoTasa(
                 instrumentoFondoAlternativoDTO.IdFondoAlternativo, instrumentofondoAlternativoTasaDTO.Seccion, TasaHasta, instrumentofondoAlternativoTasaDTO.TasaValor,
                 loginActualizacion, fechaHoraActualizacion);
            instrumentoFondoAlternativoTasa.IdFondoAlternativoTasa = persistedTasa.IdFondoAlternativoTasa;
            iFondoAlternativoTasaRepository.Merge(persistedTasa, instrumentoFondoAlternativoTasa);
            iFondoAlternativoTasaRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_ActualizarElemento;
        }

        public string RemoveFondoAlternativoTasa(int idInstrumento, int idFondoAlternativo, int idFondoAlternativoTasa)
        {
            if (idFondoAlternativo == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativo"));
            if (idFondoAlternativoTasa == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativoTasa"));

            InstrumentoFondoAlternativoTasa fondoAlternativoTasa = iFondoAlternativoTasaRepository.GetFiltered(x => x.IdFondoAlternativo == idFondoAlternativo && x.IdFondoAlternativoTasa == idFondoAlternativoTasa).First();

            if (fondoAlternativoTasa == null)
                throw new Exception(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            //bool hasExistingDependencies = iMaestroComisionDataRepository.MaestroComisionRangoHasExistingDependencies(idMaestroComision, idMaestroComisionRango);
            //if (hasExistingDependencies)
            //    throw new ApplicationException(mensajeGenericoES.MSJ_002_MaestroComision_Operacion_Relacionada);

            iFondoAlternativoTasaRepository.Remove(fondoAlternativoTasa);
            iFondoAlternativoTasaRepository.UnitOfWork.Commit();

            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO = new InstrumentoFondoAlternativoDTO();
            instrumentoFondoAlternativoDTO = GetByIdInstrumentoFondoAlternativo(idInstrumento, idFondoAlternativo);
            instrumentoFondoAlternativoDTO.TasaList = GetFondoAlternativoTasaByIdFondoAlternativo(idFondoAlternativo).ToArray();
            if (instrumentoFondoAlternativoDTO.TasaList != null)
            {
                foreach (var tasaList in instrumentoFondoAlternativoDTO.TasaList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.Descripcion.ToLower() == tasaList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = (i - 1) == 0 ? 1 : (i - 1);
                    tasaList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).Descripcion;
                    UpdateFondoAlternativoTasa(tasaList, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, instrumentoFondoAlternativoDTO.FechaHoraActualizacion);
                }
            }

            return mensajeGenericoES.exito_EliminarElemento;
        }

        private List<InstrumentoFondoAlternativoTasaDTO> GetFondoAlternativoTasaByIdFondoAlternativo(int idFondoAlternativo)
        {
            if (idFondoAlternativo == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            List<InstrumentoFondoAlternativoTasa> fondoAlternativoTasa = iFondoAlternativoTasaRepository.GetFiltered(x => x.IdFondoAlternativo == idFondoAlternativo).OrderBy(obj => obj.Seccion).ToList();
            return fondoAlternativoTasa.ProjectedAsCollection<InstrumentoFondoAlternativoTasaDTO>();
        }

        #endregion

        #region FondoAlternativoComprometido Members

        public int AddNewFondoAlternativoComprometido(InstrumentoFondoAlternativoComprometidoDTO fondoAlternativoComprometidoDTO,
            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (fondoAlternativoComprometidoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoComprometidoDTO"));

            if (instrumentoFondoAlternativoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoDTO"));

            int MaxInt = (int)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXEntero).ValorAuxNum1;
            //MAX
            int AnhioA = (fondoAlternativoComprometidoDTO.AnhioA.Equals(-1)) ? MaxInt : fondoAlternativoComprometidoDTO.AnhioA;

            if (instrumentoFondoAlternativoDTO.ComprometidoList != null)
            {
                foreach (var comprometidoList in instrumentoFondoAlternativoDTO.ComprometidoList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.ValorAuxChar1.ToLower() == comprometidoList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = i + 1;
                    comprometidoList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).ValorAuxChar1;
                    UpdateFondoAlternativoComprometido(comprometidoList, instrumentoFondoAlternativoDTO, loginActualizacion, fechaHoraActualizacion);
                }
            }

            InstrumentoFondoAlternativoComprometido fondoAlternativoComprometido = new InstrumentoFondoAlternativoComprometido(fondoAlternativoComprometidoDTO.IdFondoAlternativo,
                fondoAlternativoComprometidoDTO.Seccion, fondoAlternativoComprometidoDTO.AnhioDe, AnhioA, fondoAlternativoComprometidoDTO.MontoFijo, fondoAlternativoComprometidoDTO.FlagMarginal,
                loginActualizacion, fechaHoraActualizacion);

            SaveFondoAlternativoComprometido(fondoAlternativoComprometido);

            return fondoAlternativoComprometido.IdFondoAlternativoComprometido;
        }


        public string UpdateFondoAlternativoComprometido(InstrumentoFondoAlternativoComprometidoDTO instrumentofondoAlternativoComprometidoDTO,
            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (instrumentofondoAlternativoComprometidoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentofondoAlternativoComprometidoDTO"));

            if (instrumentoFondoAlternativoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoDTO"));

            int MaxInt = (int)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXEntero).ValorAuxNum1;
            //MAX
            int AnhioA = (instrumentofondoAlternativoComprometidoDTO.AnhioA.Equals(-1)) ? MaxInt : instrumentofondoAlternativoComprometidoDTO.AnhioA;

            InstrumentoFondoAlternativoComprometido persistedComprometido = iFondoAlternativoComprometidoRepository.Get(instrumentofondoAlternativoComprometidoDTO.IdFondoAlternativoComprometido);
            InstrumentoFondoAlternativoComprometido instrumentoFondoAlternativoComprometido = new InstrumentoFondoAlternativoComprometido(
                 instrumentoFondoAlternativoDTO.IdFondoAlternativo, instrumentofondoAlternativoComprometidoDTO.Seccion, instrumentofondoAlternativoComprometidoDTO.AnhioDe, AnhioA, instrumentofondoAlternativoComprometidoDTO.MontoFijo,
                 instrumentofondoAlternativoComprometidoDTO.FlagMarginal, loginActualizacion, fechaHoraActualizacion);
            instrumentoFondoAlternativoComprometido.IdFondoAlternativoComprometido = persistedComprometido.IdFondoAlternativoComprometido;
            iFondoAlternativoComprometidoRepository.Merge(persistedComprometido, instrumentoFondoAlternativoComprometido);
            iFondoAlternativoComprometidoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_ActualizarElemento;
        }

        public string RemoveFondoAlternativoComprometido(int idInstrumento, int idFondoAlternativo, int idFondoAlternativoComprometido)
        {
            if (idFondoAlternativo == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativo"));
            if (idFondoAlternativoComprometido == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativoComprometido"));

            InstrumentoFondoAlternativoComprometido fondoAlternativoComprometido = iFondoAlternativoComprometidoRepository.GetFiltered(x => x.IdFondoAlternativo == idFondoAlternativo && x.IdFondoAlternativoComprometido == idFondoAlternativoComprometido).First();

            if (fondoAlternativoComprometido == null)
                throw new Exception(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            //bool hasExistingDependencies = iMaestroComisionDataRepository.MaestroComisionRangoHasExistingDependencies(idMaestroComision, idMaestroComisionRango);
            //if (hasExistingDependencies)
            //    throw new ApplicationException(mensajeGenericoES.MSJ_002_MaestroComision_Operacion_Relacionada);

            iFondoAlternativoComprometidoRepository.Remove(fondoAlternativoComprometido);
            iFondoAlternativoComprometidoRepository.UnitOfWork.Commit();


            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO = new InstrumentoFondoAlternativoDTO();
            instrumentoFondoAlternativoDTO = GetByIdInstrumentoFondoAlternativo(idInstrumento, idFondoAlternativo);
            instrumentoFondoAlternativoDTO.ComprometidoList = GetFondoAlternativoComprometidoByIdFondoAlternativo(idFondoAlternativo).ToArray();
            if (instrumentoFondoAlternativoDTO.ComprometidoList != null)
            {
                foreach (var comprometidoList in instrumentoFondoAlternativoDTO.ComprometidoList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.ValorAuxChar1.ToLower() == comprometidoList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = (i - 1) == 0 ? 1 : (i - 1);
                    comprometidoList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).ValorAuxChar1;
                    UpdateFondoAlternativoComprometido(comprometidoList, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, instrumentoFondoAlternativoDTO.FechaHoraActualizacion);
                }
            }

            return mensajeGenericoES.exito_EliminarElemento;
        }

        private List<InstrumentoFondoAlternativoComprometidoDTO> GetFondoAlternativoComprometidoByIdFondoAlternativo(int idFondoAlternativo)
        {
            if (idFondoAlternativo == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            List<InstrumentoFondoAlternativoComprometido> fondoAlternativoComprometido = iFondoAlternativoComprometidoRepository.GetFiltered(x => x.IdFondoAlternativo == idFondoAlternativo).ToList();
            return fondoAlternativoComprometido.ProjectedAsCollection<InstrumentoFondoAlternativoComprometidoDTO>();
        }

        #endregion

        #region FondoAlternativoLlamada Members

        public int AddNewFondoAlternativoLlamada(InstrumentoFondoAlternativoLlamadaDTO fondoAlternativoLlamadaDTO,
            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (fondoAlternativoLlamadaDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoLlamadaDTO"));

            if (instrumentoFondoAlternativoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoDTO"));

            int MaxInt = (int)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXEntero).ValorAuxNum1;
            //MAX
            int AnhioA = (fondoAlternativoLlamadaDTO.AnhioA.Equals(-1)) ? MaxInt : fondoAlternativoLlamadaDTO.AnhioA;

            if (instrumentoFondoAlternativoDTO.LlamadaList != null)
            {
                foreach (var LlamadaList in instrumentoFondoAlternativoDTO.LlamadaList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.ValorAuxChar1.ToLower() == LlamadaList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = i + 1;
                    LlamadaList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).ValorAuxChar1;
                    UpdateFondoAlternativoLlamada(LlamadaList, instrumentoFondoAlternativoDTO, loginActualizacion, fechaHoraActualizacion);
                }
            }

            InstrumentoFondoAlternativoLlamada fondoAlternativoLlamada = new InstrumentoFondoAlternativoLlamada(fondoAlternativoLlamadaDTO.IdFondoAlternativo,
                fondoAlternativoLlamadaDTO.Seccion, fondoAlternativoLlamadaDTO.AnhioDe, AnhioA, fondoAlternativoLlamadaDTO.MontoFijo, fondoAlternativoLlamadaDTO.FlagMarginal,
                loginActualizacion, fechaHoraActualizacion);

            SaveFondoAlternativoLlamada(fondoAlternativoLlamada);

            return fondoAlternativoLlamada.IdFondoAlternativoLlamada;
        }


        public string UpdateFondoAlternativoLlamada(InstrumentoFondoAlternativoLlamadaDTO instrumentofondoAlternativoLlamadaDTO,
            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (instrumentofondoAlternativoLlamadaDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentofondoAlternativoLlamadaDTO"));

            if (instrumentoFondoAlternativoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "instrumentoFondoAlternativoDTO"));

            int MaxInt = (int)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXEntero).ValorAuxNum1;
            //MAX
            int AnhioA = (instrumentofondoAlternativoLlamadaDTO.AnhioA.Equals(-1)) ? MaxInt : instrumentofondoAlternativoLlamadaDTO.AnhioA;

            InstrumentoFondoAlternativoLlamada persistedLlamada = iFondoAlternativoLlamadaRepository.Get(instrumentofondoAlternativoLlamadaDTO.IdFondoAlternativoLlamada);
            InstrumentoFondoAlternativoLlamada instrumentoFondoAlternativoLlamada = new InstrumentoFondoAlternativoLlamada(
                 instrumentoFondoAlternativoDTO.IdFondoAlternativo, instrumentofondoAlternativoLlamadaDTO.Seccion, instrumentofondoAlternativoLlamadaDTO.AnhioDe, AnhioA, instrumentofondoAlternativoLlamadaDTO.MontoFijo,
                 instrumentofondoAlternativoLlamadaDTO.FlagMarginal, loginActualizacion, fechaHoraActualizacion);
            instrumentoFondoAlternativoLlamada.IdFondoAlternativoLlamada = persistedLlamada.IdFondoAlternativoLlamada;
            iFondoAlternativoLlamadaRepository.Merge(persistedLlamada, instrumentoFondoAlternativoLlamada);
            iFondoAlternativoLlamadaRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_ActualizarElemento;
        }

        public string RemoveFondoAlternativoLlamada(int idInstrumento, int idFondoAlternativo, int idFondoAlternativoLlamada)
        {
            if (idFondoAlternativo == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativo"));
            if (idFondoAlternativoLlamada == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativoLlamada"));

            InstrumentoFondoAlternativoLlamada fondoAlternativoLlamada = iFondoAlternativoLlamadaRepository.GetFiltered(x => x.IdFondoAlternativo == idFondoAlternativo && x.IdFondoAlternativoLlamada == idFondoAlternativoLlamada).First();

            if (fondoAlternativoLlamada == null)
                throw new Exception(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            //bool hasExistingDependencies = iMaestroComisionDataRepository.MaestroComisionRangoHasExistingDependencies(idMaestroComision, idMaestroComisionRango);
            //if (hasExistingDependencies)
            //    throw new ApplicationException(mensajeGenericoES.MSJ_002_MaestroComision_Operacion_Relacionada);

            iFondoAlternativoLlamadaRepository.Remove(fondoAlternativoLlamada);
            iFondoAlternativoLlamadaRepository.UnitOfWork.Commit();


            InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO = new InstrumentoFondoAlternativoDTO();
            instrumentoFondoAlternativoDTO = GetByIdInstrumentoFondoAlternativo(idInstrumento, idFondoAlternativo);
            instrumentoFondoAlternativoDTO.LlamadaList = GetFondoAlternativoLlamadaByIdFondoAlternativo(idFondoAlternativo).ToArray();
            if (instrumentoFondoAlternativoDTO.LlamadaList != null)
            {
                foreach (var LlamadaList in instrumentoFondoAlternativoDTO.LlamadaList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.ValorAuxChar1.ToLower() == LlamadaList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = (i - 1) == 0 ? 1 : (i - 1);
                    LlamadaList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).ValorAuxChar1;
                    UpdateFondoAlternativoLlamada(LlamadaList, instrumentoFondoAlternativoDTO, instrumentoFondoAlternativoDTO.LoginActualizacion, instrumentoFondoAlternativoDTO.FechaHoraActualizacion);
                }
            }

            return mensajeGenericoES.exito_EliminarElemento;
        }

        private List<InstrumentoFondoAlternativoLlamadaDTO> GetFondoAlternativoLlamadaByIdFondoAlternativo(int idFondoAlternativo)
        {
            if (idFondoAlternativo == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            List<InstrumentoFondoAlternativoLlamada> fondoAlternativoLlamada = iFondoAlternativoLlamadaRepository.GetFiltered(x => x.IdFondoAlternativo == idFondoAlternativo).ToList();
            return fondoAlternativoLlamada.ProjectedAsCollection<InstrumentoFondoAlternativoLlamadaDTO>();
        }

        #endregion

        #region FondoAlternativoDetalleComprometido Members

        public string AddNewFondoAlternativoDetalleComprometido(InstrumentoFondoAlternativoDetalleComprometidoDTO fondoAlternativoDetalleComprometidoDTO,
           InstrumentoFondoAlternativoComprometidoDTO instrumentoFondoAlternativoComprometidoDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (fondoAlternativoDetalleComprometidoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoDetalleComprometidoDTO"));

            decimal MaxDecimal = (decimal)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXDecimal).ValorAuxDecimal1;
            //MAX
            decimal TasaHasta = (fondoAlternativoDetalleComprometidoDTO.TasaHasta.Equals(-1)) ? MaxDecimal : fondoAlternativoDetalleComprometidoDTO.TasaHasta;

            if (instrumentoFondoAlternativoComprometidoDTO.DetalleComprometidoList != null)
            {
                foreach (var comprometidoDetailList in instrumentoFondoAlternativoComprometidoDTO.DetalleComprometidoList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.Descripcion.ToLower() == comprometidoDetailList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = i + 1;
                    comprometidoDetailList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).Descripcion;
                    UpdateFondoAlternativoDetalleComprometido(comprometidoDetailList, instrumentoFondoAlternativoComprometidoDTO, loginActualizacion, fechaHoraActualizacion);
                }
            }

            InstrumentoFondoAlternativoDetalleComprometido fondoAlternativoDetalleComprometido = new InstrumentoFondoAlternativoDetalleComprometido(fondoAlternativoDetalleComprometidoDTO.IdFondoAlternativoComprometido,
                fondoAlternativoDetalleComprometidoDTO.Seccion, TasaHasta, fondoAlternativoDetalleComprometidoDTO.TasaValor,
                loginActualizacion, fechaHoraActualizacion);

            SaveFondoAlternativoDetalleComprometido(fondoAlternativoDetalleComprometido);

            return mensajeGenericoES.exito_RegistrarElemento;
        }

        public string UpdateFondoAlternativoDetalleComprometido(InstrumentoFondoAlternativoDetalleComprometidoDTO fondoAlternativoDetalleComprometidoDTO,
            InstrumentoFondoAlternativoComprometidoDTO fondoAlternativoComprometidoDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (fondoAlternativoDetalleComprometidoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoDetalleComprometidoDTO"));

            if (fondoAlternativoComprometidoDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoComprometidoDTO"));

            decimal MaxDecimal = (decimal)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXDecimal).ValorAuxDecimal1;
            //MAX
            decimal TasaHasta = (fondoAlternativoDetalleComprometidoDTO.TasaHasta.Equals(-1)) ? MaxDecimal : fondoAlternativoDetalleComprometidoDTO.TasaHasta;

            InstrumentoFondoAlternativoDetalleComprometido persistedDetalleComprometido = iFondoAlternativoDetalleComprometidoRepository.Get(fondoAlternativoDetalleComprometidoDTO.IdFondoAlternativoDetalleComprometido);
            InstrumentoFondoAlternativoDetalleComprometido instrumentoFondoAlternativoDetalleComprometido = new InstrumentoFondoAlternativoDetalleComprometido(
                 persistedDetalleComprometido.IdFondoAlternativoComprometido, fondoAlternativoDetalleComprometidoDTO.Seccion, TasaHasta, fondoAlternativoDetalleComprometidoDTO.TasaValor,
                 loginActualizacion, fechaHoraActualizacion);
            instrumentoFondoAlternativoDetalleComprometido.IdFondoAlternativoDetalleComprometido = persistedDetalleComprometido.IdFondoAlternativoDetalleComprometido;
            iFondoAlternativoDetalleComprometidoRepository.Merge(persistedDetalleComprometido, instrumentoFondoAlternativoDetalleComprometido);
            iFondoAlternativoDetalleComprometidoRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_ActualizarElemento;
        }

        public string RemoveFondoAlternativoDetalleComprometido(int idFondoAlternativo, int idFondoAlternativoComprometido, int idFondoAlternativoDetalleComprometido)
        {
            if (idFondoAlternativoComprometido == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativoComprometido"));
            if (idFondoAlternativoDetalleComprometido == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativoDetalleComprometido"));

            InstrumentoFondoAlternativoDetalleComprometido fondoAlternativoDetalleComprometido = iFondoAlternativoDetalleComprometidoRepository.GetFiltered(x => x.IdFondoAlternativoComprometido == idFondoAlternativoComprometido && x.IdFondoAlternativoDetalleComprometido == idFondoAlternativoDetalleComprometido).First();

            if (fondoAlternativoDetalleComprometido == null)
                throw new Exception(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            //bool hasExistingDependencies = iMaestroComisionDataRepository.MaestroComisionRangoHasExistingDependencies(idMaestroComision, idMaestroComisionRango);
            //if (hasExistingDependencies)
            //    throw new ApplicationException(mensajeGenericoES.MSJ_002_MaestroComision_Operacion_Relacionada);

            iFondoAlternativoDetalleComprometidoRepository.Remove(fondoAlternativoDetalleComprometido);
            iFondoAlternativoDetalleComprometidoRepository.UnitOfWork.Commit();


            InstrumentoFondoAlternativoComprometidoDTO instrumentoFondoAlternativoComprometidoDTO = new InstrumentoFondoAlternativoComprometidoDTO();
            instrumentoFondoAlternativoComprometidoDTO = GetFondoAlternativoComprometidoByIdFondoAlternativo(idFondoAlternativo).Where(x => x.IdFondoAlternativoComprometido.Equals(idFondoAlternativoComprometido)).FirstOrDefault();
            instrumentoFondoAlternativoComprometidoDTO.DetalleComprometidoList = GetFondoAlternativoDetalleComprometidoByIdFondoAlternativoComprometido(idFondoAlternativoComprometido).ToArray();
            if (instrumentoFondoAlternativoComprometidoDTO.DetalleComprometidoList != null)
            {
                foreach (var comprometidoDetailList in instrumentoFondoAlternativoComprometidoDTO.DetalleComprometidoList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.Descripcion.ToLower() == comprometidoDetailList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = (i - 1) == 0 ? 1 : (i - 1);
                    comprometidoDetailList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).Descripcion;
                    UpdateFondoAlternativoDetalleComprometido(comprometidoDetailList, instrumentoFondoAlternativoComprometidoDTO, instrumentoFondoAlternativoComprometidoDTO.LoginActualizacion, instrumentoFondoAlternativoComprometidoDTO.FechaHoraActualizacion);
                }
            }

            return mensajeGenericoES.exito_EliminarElemento;
        }

        private List<InstrumentoFondoAlternativoDetalleComprometidoDTO> GetFondoAlternativoDetalleComprometidoByIdFondoAlternativoComprometido(int idFondoAlternativoComprometido)
        {
            if (idFondoAlternativoComprometido == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            List<InstrumentoFondoAlternativoDetalleComprometido> fondoAlternativoDetalleComprometido = iFondoAlternativoDetalleComprometidoRepository.GetFiltered(x => x.IdFondoAlternativoComprometido == idFondoAlternativoComprometido).OrderBy(obj => obj.Seccion).ToList();
            return fondoAlternativoDetalleComprometido.ProjectedAsCollection<InstrumentoFondoAlternativoDetalleComprometidoDTO>();
        }

        #endregion

        #region FondoAlternativoDetalleLlamada Members

        public string AddNewFondoAlternativoDetalleLlamada(InstrumentoFondoAlternativoDetalleLlamadaDTO fondoAlternativoDetalleLlamadaDTO,
            InstrumentoFondoAlternativoLlamadaDTO instrumentoFondoAlternativoLlamadaDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (fondoAlternativoDetalleLlamadaDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoDetalleLlamadaDTO"));

            decimal MaxDecimal = (decimal)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXDecimal).ValorAuxDecimal1;
            //MAX
            decimal TasaHasta = (fondoAlternativoDetalleLlamadaDTO.TasaHasta.Equals(-1)) ? MaxDecimal : fondoAlternativoDetalleLlamadaDTO.TasaHasta;

            if (instrumentoFondoAlternativoLlamadaDTO.DetalleLlamadaList != null)
            {
                foreach (var LlamadaDetailList in instrumentoFondoAlternativoLlamadaDTO.DetalleLlamadaList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.Descripcion.ToLower() == LlamadaDetailList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = i + 1;
                    LlamadaDetailList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).Descripcion;
                    UpdateFondoAlternativoDetalleLlamada(LlamadaDetailList, instrumentoFondoAlternativoLlamadaDTO, loginActualizacion, fechaHoraActualizacion);
                }
            }

            InstrumentoFondoAlternativoDetalleLlamada fondoAlternativoDetalleLlamada = new InstrumentoFondoAlternativoDetalleLlamada(fondoAlternativoDetalleLlamadaDTO.IdFondoAlternativoLlamada,
                fondoAlternativoDetalleLlamadaDTO.Seccion, TasaHasta, fondoAlternativoDetalleLlamadaDTO.TasaValor,
                loginActualizacion, fechaHoraActualizacion);

            SaveFondoAlternativoDetalleLlamada(fondoAlternativoDetalleLlamada);

            return mensajeGenericoES.exito_RegistrarElemento;
        }

        public string UpdateFondoAlternativoDetalleLlamada(InstrumentoFondoAlternativoDetalleLlamadaDTO fondoAlternativoDetalleLlamadaDTO,
            InstrumentoFondoAlternativoLlamadaDTO fondoAlternativoLlamadaDTO, string loginActualizacion, DateTime fechaHoraActualizacion)
        {
            if (fondoAlternativoDetalleLlamadaDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoDetalleLlamadaDTO"));

            if (fondoAlternativoLlamadaDTO == null)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "fondoAlternativoLlamadaDTO"));

            decimal MaxDecimal = (decimal)iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.MaxValue, (int)eMaxValue.MAXDecimal).ValorAuxDecimal1;
            //MAX
            decimal TasaHasta = (fondoAlternativoDetalleLlamadaDTO.TasaHasta.Equals(-1)) ? MaxDecimal : fondoAlternativoDetalleLlamadaDTO.TasaHasta;

            InstrumentoFondoAlternativoDetalleLlamada persistedDetalleLlamada = iFondoAlternativoDetalleLlamadaRepository.Get(fondoAlternativoDetalleLlamadaDTO.IdFondoAlternativoDetalleLlamada);
            InstrumentoFondoAlternativoDetalleLlamada instrumentoFondoAlternativoDetalleLlamada = new InstrumentoFondoAlternativoDetalleLlamada(
                 persistedDetalleLlamada.IdFondoAlternativoLlamada, fondoAlternativoDetalleLlamadaDTO.Seccion, TasaHasta, fondoAlternativoDetalleLlamadaDTO.TasaValor,
                 loginActualizacion, fechaHoraActualizacion);
            instrumentoFondoAlternativoDetalleLlamada.IdFondoAlternativoDetalleLlamada = persistedDetalleLlamada.IdFondoAlternativoDetalleLlamada;
            iFondoAlternativoDetalleLlamadaRepository.Merge(persistedDetalleLlamada, instrumentoFondoAlternativoDetalleLlamada);
            iFondoAlternativoDetalleLlamadaRepository.UnitOfWork.Commit();

            return mensajeGenericoES.exito_ActualizarElemento;
        }

        public string RemoveFondoAlternativoDetalleLlamada(int idFondoAlternativo, int idFondoAlternativoLlamada, int idFondoAlternativoDetalleLlamada)
        {
            if (idFondoAlternativoLlamada == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativoLlamada"));
            if (idFondoAlternativoDetalleLlamada == 0)
                throw new ArgumentException(string.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "idFondoAlternativoDetalleLlamada"));

            InstrumentoFondoAlternativoDetalleLlamada fondoAlternativoDetalleLlamada = iFondoAlternativoDetalleLlamadaRepository.GetFiltered(x => x.IdFondoAlternativoLlamada == idFondoAlternativoLlamada && x.IdFondoAlternativoDetalleLlamada == idFondoAlternativoDetalleLlamada).First();

            if (fondoAlternativoDetalleLlamada == null)
                throw new Exception(mensajeGenericoES.error_SeleccionarElementoYaNoExiste);

            //bool hasExistingDependencies = iMaestroComisionDataRepository.MaestroComisionRangoHasExistingDependencies(idMaestroComision, idMaestroComisionRango);
            //if (hasExistingDependencies)
            //    throw new ApplicationException(mensajeGenericoES.MSJ_002_MaestroComision_Operacion_Relacionada);

            iFondoAlternativoDetalleLlamadaRepository.Remove(fondoAlternativoDetalleLlamada);
            iFondoAlternativoDetalleLlamadaRepository.UnitOfWork.Commit();


            InstrumentoFondoAlternativoLlamadaDTO instrumentoFondoAlternativoLlamadaDTO = new InstrumentoFondoAlternativoLlamadaDTO();
            instrumentoFondoAlternativoLlamadaDTO = GetFondoAlternativoLlamadaByIdFondoAlternativo(idFondoAlternativo).Where(x => x.IdFondoAlternativoLlamada.Equals(idFondoAlternativoLlamada)).FirstOrDefault();
            instrumentoFondoAlternativoLlamadaDTO.DetalleLlamadaList = GetFondoAlternativoDetalleLlamadaByIdFondoAlternativoLlamada(idFondoAlternativoLlamada).ToArray();
            if (instrumentoFondoAlternativoLlamadaDTO.DetalleLlamadaList != null)
            {
                foreach (var LlamadaDetailList in instrumentoFondoAlternativoLlamadaDTO.DetalleLlamadaList)
                {
                    var result = iIndicadorRepository.GetFiltered(x => x.TipoIndicador == (int)eIndicador.FechaTransferencia && x.Descripcion.ToLower() == LlamadaDetailList.Seccion.Trim().ToLower());
                    var i = 0;
                    if (result.Count() > 0)
                        i = result.First().IdIndicador;
                    i = (i - 1) == 0 ? 1 : (i - 1);
                    LlamadaDetailList.Seccion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.FechaTransferencia, i).Descripcion;
                    UpdateFondoAlternativoDetalleLlamada(LlamadaDetailList, instrumentoFondoAlternativoLlamadaDTO, instrumentoFondoAlternativoLlamadaDTO.LoginActualizacion, instrumentoFondoAlternativoLlamadaDTO.FechaHoraActualizacion);
                }
            }


            return mensajeGenericoES.exito_EliminarElemento;
        }

        private List<InstrumentoFondoAlternativoDetalleLlamadaDTO> GetFondoAlternativoDetalleLlamadaByIdFondoAlternativoLlamada(int idFondoAlternativoLlamada)
        {
            if (idFondoAlternativoLlamada == 0)
                throw new Exception(mensajeGenericoES.error_ObtenerIdCero);

            List<InstrumentoFondoAlternativoDetalleLlamada> fondoAlternativoDetalleLlamada = iFondoAlternativoDetalleLlamadaRepository.GetFiltered(x => x.IdFondoAlternativoLlamada == idFondoAlternativoLlamada).OrderBy(obj => obj.Seccion).ToList();
            return fondoAlternativoDetalleLlamada.ProjectedAsCollection<InstrumentoFondoAlternativoDetalleLlamadaDTO>();
        }

        #endregion

        #region Cuponera Members
        public InstrumentoCuponeraListadoDTO[] GetPreviewCuponera(InstrumentoCuponeraDTO instrumentoCuponeraDTO)
        {
            List<InstrumentoCuponeraListadoDTO> listaCupones = new List<InstrumentoCuponeraListadoDTO>();

            if (instrumentoCuponeraDTO.IdTipoTasaCalculo == Convert.ToInt32(eTipoTasaCalculo.SOFR))
            {
                return GetPreviewCuponeraSOFR(instrumentoCuponeraDTO);
            }
            else
            {

                IndicadorDTO indicadorInteres = new IndicadorDTO();
                IndicadorDTO indicadorPeriodoGracia = new IndicadorDTO();
                IndicadorDTO indicadorTipoAmortizacion = new IndicadorDTO();
                IndicadorDTO indicadorBaseCalculo = new IndicadorDTO();
                IndicadorDTO indicadorPeriodoPago = new IndicadorDTO();
                IndicadorDTO indicadorIndexadoInflacion = new IndicadorDTO();


                MonedaDTO monedaIndexada = new MonedaDTO();
                int numeroCupon = 1;

                int indicadorTipoCalculoInteres = instrumentoCuponeraDTO.IdTipoCalculoInteres;
                int indicadorTipoInteres = instrumentoCuponeraDTO.IdTipoInteres;



                VerifyDatosCuponera(instrumentoCuponeraDTO, out indicadorInteres, out indicadorPeriodoGracia, out indicadorTipoAmortizacion, out indicadorBaseCalculo, out indicadorPeriodoPago, out monedaIndexada, out indicadorIndexadoInflacion);
                int cantidadMesesPeriodoPago = Convert.ToInt32(indicadorPeriodoPago.ValorAuxDecimal1 == null ? 0 : indicadorPeriodoPago.ValorAuxDecimal1.Value);
                int cantidadDiasPeriodoPago = Convert.ToInt32(indicadorPeriodoPago.ValorAuxNum1.Value);
                var IdiDTO = iIdiAppService.GetFirstIDIOpenWhenAllFondoPensionIsOpen();
                var fecha = Helper.ConvertIdFechaToDateTime(IdiDTO.IdSecuencialFechaIDI);
                var Stock = decimal.Zero;


                if (instrumentoCuponeraDTO.IdInstrumento != null && instrumentoCuponeraDTO.IdInstrumento != 0)
                {
                    var dataStock = iInstrumentoStockRepository.GetFiltered(x => x.IdInstrumento == instrumentoCuponeraDTO.IdInstrumento && x.IdSecuencialFecha == IdiDTO.IdSecuencialFechaIDI).ToArray();
                    if (dataStock.Any())
                        Stock = dataStock.Sum(x => x.SaldoContable);
                }


                if (indicadorPeriodoPago.IdIndicador.Equals((int)ePeriodoPago.Vencimiento) || indicadorPeriodoPago.IdIndicador.Equals((int)ePeriodoPagoCertificadoDeposito.Vencimiento))
                {
                    InstrumentoCuponeraListadoDTO cupon = new InstrumentoCuponeraListadoDTO();
                    cupon.NumeroCupon = numeroCupon;
                    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime;
                    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaVencimientoDateTime;
                    cupon.FechaPagoDate = ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, cupon.FechaCorteDate);
                    cupon.Dias = (cupon.FechaCorteDate - cupon.FechaInicioDate).Days;
                    cupon.Indicador = cupon.FechaCorteDate >= fecha ? eEstadoVigenciaCupon.Vigente.ToString() : eEstadoVigenciaCupon.Vencido.ToString();
                    cupon.IdIndicador = cupon.FechaCorteDate >= fecha ? (int)eEstadoVigenciaCupon.Vigente : (int)eEstadoVigenciaCupon.Vencido;
                    cupon.Factor = monedaIndexada.IdMoneda == 0 ? decimal.Zero : ObtenerFactor(monedaIndexada.IdMoneda, cupon.FechaInicioDate, cupon.FechaCorteDate);
                    cupon.Tasa = ObtenerTasaCupon(instrumentoCuponeraDTO.OrdenCambioTasa, instrumentoCuponeraDTO.TasaFijaCupon, instrumentoCuponeraDTO.IdTasaLibor, instrumentoCuponeraDTO.TasaVariableCupon, instrumentoCuponeraDTO.FechaCambioTipoTasaDateTime, cupon.FechaInicioDate, indicadorIndexadoInflacion, monedaIndexada.IdMoneda, cupon.FechaCorteDate);

                    cupon.EstadoCupon = eEstadoCupon.Calculado.ToString();
                    cupon.IdIndicadorEstadoCupon = (int)eEstadoCupon.Calculado;
                    listaCupones.Add(cupon);
                }
                else
                {
                    //Calculamos los valores que pueden obtenerse sin saber la cantidad total de cupones
                    while ((listaCupones.Count == 0 ? DateTime.MinValue : listaCupones.Last().FechaCorteDate) < instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime)
                    {
                        bool calcularDiferenciaDias = false;
                        InstrumentoCuponeraListadoDTO cupon = new InstrumentoCuponeraListadoDTO();
                        cupon.NumeroCupon = numeroCupon;
                        if (instrumentoCuponeraDTO.MesesCalendario)
                        {
                            #region comentado
                            //if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                            //{
                            //    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * listaCupones.Count);
                            //    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * (listaCupones.Count + 1)).AddDays(-1);
                            //}
                            //else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                            //{
                            //    cupon.FechaInicioDate = listaCupones.Count == 0 ? instrumentoCuponeraDTO.FechaEmisionDateTime : listaCupones.Last().FechaCorteDate.AddDays(1);
                            //    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime.AddMonths(cantidadMesesPeriodoPago * listaCupones.Count);
                            //}
                            ////Establecemos la fecha de inicio del primer cupon en caso se haya personalizado
                            //if (listaCupones.Count == 0 && !string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon) && !instrumentoCuponeraDTO.FechaEmision.Equals(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                            //    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                            #endregion
                            if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                            {
                                if (listaCupones.Count.Equals(0))
                                    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                                else
                                    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * listaCupones.Count).AddDays(1);
                                cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                            }
                            else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                            {
                                cupon.FechaInicioDate = listaCupones.Count == 0 ? instrumentoCuponeraDTO.FechaEmisionDateTime.AddDays(1) : listaCupones.Last().FechaCorteDate.AddDays(1);
                                cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime.AddMonths(cantidadMesesPeriodoPago * listaCupones.Count);
                            }
                        }
                        else if (instrumentoCuponeraDTO.PersonalizaDiasMes)
                        {
                            #region comentado
                            //if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                            //{
                            //    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago * listaCupones.Count);
                            //    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago * (listaCupones.Count + 1)).AddDays(-1);
                            //}
                            //else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                            //{
                            //    cupon.FechaInicioDate = listaCupones.Count == 0 ? instrumentoCuponeraDTO.FechaEmisionDateTime : listaCupones.Last().FechaCorteDate.AddDays(1);
                            //    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago * listaCupones.Count);
                            //}
                            #endregion
                            if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                            {
                                if (listaCupones.Count.Equals(0))
                                {
                                    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                                    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                                }
                                else
                                {
                                    cupon.FechaInicioDate = listaCupones.Last().FechaCorteDate.AddDays(1);
                                    cupon.FechaCorteDate = cupon.FechaInicioDate.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago).AddDays(-1);
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                            {
                                if (listaCupones.Count.Equals(0))
                                {
                                    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddDays(1);
                                    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime;
                                }
                                else
                                {
                                    cupon.FechaInicioDate = listaCupones.Last().FechaCorteDate.AddDays(1);
                                    cupon.FechaCorteDate = cupon.FechaInicioDate.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago);
                                }
                            }
                        }
                        else if (instrumentoCuponeraDTO.FinesMes)
                        {
                            #region
                            //if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                            //{
                            //    cupon.FechaInicioDate = listaCupones.Count == 0 ? instrumentoCuponeraDTO.FechaEmisionDateTime : listaCupones.Last().FechaCorteDate.AddDays(1);
                            //    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime.AddMonths(cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                            //    //Establecemos la fecha de fin de mes
                            //    cupon.FechaCorteDate = new DateTime(cupon.FechaCorteDate.Year, cupon.FechaCorteDate.Month, 1).AddMonths(1).AddDays(-1);
                            //}
                            //else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                            //{
                            //    cupon.FechaInicioDate = listaCupones.Count == 0 ? instrumentoCuponeraDTO.FechaEmisionDateTime : listaCupones.Last().FechaCorteDate.AddDays(1);
                            //    cupon.FechaCorteDate = listaCupones.Count == 0 ? instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime : cupon.FechaInicioDate.AddMonths(cantidadMesesPeriodoPago).AddDays(-1);
                            //    //Establecemos la fecha de fin de mes dependiendo si es el primer cupon o no
                            //    if (listaCupones.Count > 0)
                            //        cupon.FechaCorteDate = new DateTime(cupon.FechaCorteDate.Year, cupon.FechaCorteDate.Month, 1).AddMonths(1).AddDays(-1);
                            //}
                            ////Establecemos la fecha de inicio del primer cupon en caso se haya personalizado
                            //if (listaCupones.Count == 0 && !string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon) && !instrumentoCuponeraDTO.FechaEmision.Equals(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                            //    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                            #endregion
                            if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                            {
                                if (listaCupones.Count.Equals(0))
                                    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                                else
                                    cupon.FechaInicioDate = listaCupones.Last().FechaCorteDate.AddDays(1);
                                cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                            }
                            else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                            {
                                //VERIFICAR ¿Como seria este caso?
                                if (listaCupones.Count.Equals(0))
                                {
                                    cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddDays(1);
                                    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime;
                                }
                                else
                                {
                                    cupon.FechaInicioDate = listaCupones.Last().FechaCorteDate.AddDays(1);
                                    cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                                }
                            }
                            cupon.FechaCorteDate = new DateTime(cupon.FechaCorteDate.Year, cupon.FechaCorteDate.Month, 1).AddMonths(1).AddDays(-1);
                        }

                        //Verificamos que la fecha no exceda la fecha de corte del último cupón
                        DateTime fechaCorteEsperada = cupon.FechaCorteDate;
                        cupon.FechaCorteDate = cupon.FechaCorteDate > instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime ? instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime : cupon.FechaCorteDate;
                        cupon.FechaPagoDate = ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, cupon.FechaCorteDate);

                        if (instrumentoCuponeraDTO.PersonalizaDiasMes || instrumentoCuponeraDTO.FinesMes || instrumentoCuponeraDTO.EsBaseCalculoActual)
                            cupon.Dias = (cupon.FechaCorteDate - cupon.FechaInicioDate).Days + 1;
                        else if (instrumentoCuponeraDTO.MesesCalendario)
                        {
                            //En caso que se haya seleccionado la fecha de inicio o corte del primer cupon y sea el primer cupon
                            if (listaCupones.Count == 0 || (cupon.FechaCorteDate.Equals(instrumentoCuponeraDTO.FechaVencimientoDateTime) && !fechaCorteEsperada.Equals(cupon.FechaCorteDate)))
                                calcularDiferenciaDias = EsComportamientoBaseCalculoActual(instrumentoCuponeraDTO.FechaEmisionDateTime, cupon.FechaInicioDate, cupon.FechaCorteDate, instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime, instrumentoCuponeraDTO.FechaVencimientoDateTime, cantidadMesesPeriodoPago);
                            if (calcularDiferenciaDias)
                                cupon.Dias = (cupon.FechaCorteDate - cupon.FechaInicioDate).Days + 1;
                            else
                                cupon.Dias = instrumentoCuponeraDTO.PersonalizaDiasMes ? instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago : cantidadDiasPeriodoPago;
                        }
                        else
                            cupon.Dias = instrumentoCuponeraDTO.PersonalizaDiasMes ? instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago : cantidadDiasPeriodoPago;

                        cupon.Indicador = Convert.ToDateTime(cupon.FechaCorteDate.ToString("dd/MM/yyyy")) >= Convert.ToDateTime(fecha.ToString("dd/MM/yyyy")) ? eEstadoVigenciaCupon.Vigente.ToString() : eEstadoVigenciaCupon.Vencido.ToString();
                        cupon.IdIndicador = Convert.ToDateTime(cupon.FechaCorteDate.ToString("dd/MM/yyyy")) >= Convert.ToDateTime(fecha.ToString("dd/MM/yyyy")) ? (int)eEstadoVigenciaCupon.Vigente : (int)eEstadoVigenciaCupon.Vencido;
                        cupon.Factor = monedaIndexada.IdMoneda == 0 ? 0 : ObtenerFactor(monedaIndexada.IdMoneda, instrumentoCuponeraDTO.FechaEmisionDateTime, cupon.FechaCorteDate);
                        if (instrumentoCuponeraDTO.EsBonoLetraHipotecaria && (indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.CeroCupon)))
                            cupon.Tasa = decimal.Zero;
                        else
                            cupon.Tasa = ObtenerTasaCupon(instrumentoCuponeraDTO.OrdenCambioTasa, instrumentoCuponeraDTO.TasaFijaCupon, instrumentoCuponeraDTO.IdTasaLibor, instrumentoCuponeraDTO.TasaVariableCupon, instrumentoCuponeraDTO.FechaCambioTipoTasaDateTime, cupon.FechaInicioDate, indicadorIndexadoInflacion, monedaIndexada.IdMoneda, cupon.FechaCorteDate);
                        cupon.EstadoCupon = eEstadoCupon.Calculado.ToString();
                        cupon.IdIndicadorEstadoCupon = (int)eEstadoCupon.Calculado;

                        if (cupon.FechaCorteDate.ConvertToIdFecha() == IdiDTO.IdSecuencialFechaIDI)
                        {
                            if (Stock == 0)
                            {
                                cupon.Indicador = eEstadoVigenciaCupon.Vencido.ToString();
                                cupon.IdIndicador = (int)eEstadoVigenciaCupon.Vencido;
                            }
                            else
                            {
                                cupon.Indicador = eEstadoVigenciaCupon.Vigente.ToString();
                                cupon.IdIndicador = (int)eEstadoVigenciaCupon.Vigente;
                            }
                        }
                        listaCupones.Add(cupon);
                        numeroCupon++;
                    }
                }




                //Calculamos todos los montos de cupon, amortizacion, intereses y saldo de amortización
                decimal valorMontoAmortizacion = decimal.Zero;
                decimal valorMontoIntereses = decimal.Zero;
                decimal valorCupon = decimal.Zero;
                decimal valorSaldoAmortizacion = decimal.Zero;
                decimal interesAcumulado = decimal.Zero;
                bool primerRegistroInteresCompuesto = true;
                int numeroCuponAplicaCalculoInteres = indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Interes) ? listaCupones.Find(lc => lc.FechaInicioDate <= instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime && lc.FechaCorteDate >= instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime).NumeroCupon + 1 : 0;
                int numeroCuponAplicaCalculoAmortizacion = indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Amortizacion) ? listaCupones.Find(lc => lc.FechaInicioDate <= instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime && lc.FechaCorteDate >= instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime).NumeroCupon + 1 : 0;


                foreach (InstrumentoCuponeraListadoDTO cuponGenerado in listaCupones)
                {
                    try
                    {
                        valorSaldoAmortizacion = cuponGenerado.NumeroCupon.Equals(1) ? instrumentoCuponeraDTO.ValorNominalVigente : listaCupones[cuponGenerado.NumeroCupon - 2].SaldoPorAmortizar;
                        if (monedaIndexada.IdMoneda != 0 && indicadorIndexadoInflacion.IdIndicador.Equals((int)eIndexadoInflacion.AlPrincipal))
                            cuponGenerado.ValorNominalVigenteIndexado = Math.Round((valorSaldoAmortizacion * cuponGenerado.Factor), 7);

                        if ((indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Interes) && cuponGenerado.NumeroCupon < numeroCuponAplicaCalculoInteres) || (instrumentoCuponeraDTO.EsBonoLetraHipotecaria && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.CeroCupon)))
                            valorMontoIntereses = decimal.Zero;
                        else
                        {
                            decimal valorBaseCalculo = ObtenerBaseCalculoCuponera(indicadorBaseCalculo, cuponGenerado.FechaInicioDate, cuponGenerado.FechaCorteDate, cuponGenerado.Dias);
                            decimal valorCalculoInteres = monedaIndexada.IdMoneda == 0 ? valorSaldoAmortizacion : cuponGenerado.ValorNominalVigenteIndexado;




                            if (instrumentoCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TNA))
                                valorMontoIntereses = valorCalculoInteres * (cuponGenerado.Tasa / 100) * valorBaseCalculo;
                            else if (instrumentoCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TEA))
                                valorMontoIntereses = Convert.ToDecimal(Convert.ToDouble(valorCalculoInteres) * (Math.Pow((1 + Convert.ToDouble(cuponGenerado.Tasa / 100)), Convert.ToDouble(valorBaseCalculo)) - 1));




                            valorMontoIntereses = Helper.TruncateDecimals(valorMontoIntereses, 7);
                        }

                        switch (indicadorTipoAmortizacion.IdIndicador)
                        {
                            case (int)eTipoAmortizacionCertificadoDeposito.Constantes:
                                //case (int)eTipoAmortizacionBonos.Constante:
                                if (indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Amortizacion) && cuponGenerado.NumeroCupon < numeroCuponAplicaCalculoAmortizacion)
                                    valorMontoAmortizacion = decimal.Zero;
                                else
                                    valorMontoAmortizacion = instrumentoCuponeraDTO.ValorNominal / listaCupones.FindAll(lc => lc.NumeroCupon >= numeroCuponAplicaCalculoAmortizacion).Count();
                                //Se agrego la condicion para que el ultimo cupon coja el saldo por amoritzar del cupon anterior.
                                if (cuponGenerado.NumeroCupon.Equals(listaCupones.Count))
                                    valorMontoAmortizacion = listaCupones.FindAll(lc => lc.NumeroCupon.Equals(cuponGenerado.NumeroCupon - 1)).First().SaldoPorAmortizar;

                                valorMontoAmortizacion = Helper.TruncateDecimals(valorMontoAmortizacion, 7);
                                valorCupon = valorMontoAmortizacion + valorMontoIntereses;
                                break;
                            case (int)eTipoAmortizacionCertificadoDeposito.CuotasIguales:
                                //case (int)eTipoAmortizacionBonos.Cuotas
                                decimal factorK = decimal.Zero;
                                decimal tasa = instrumentoCuponeraDTO.TasaFijaCupon.Equals(0) ? instrumentoCuponeraDTO.TasaVariableCupon : instrumentoCuponeraDTO.TasaFijaCupon;
                                decimal cantidadPeriodosPago = cantidadMesesPeriodoPago >= 1 ? Convert.ToDecimal(12 / cantidadMesesPeriodoPago) : Convert.ToDecimal(12 / 0.5);
                                tasa = (tasa / 100);
                                if (instrumentoCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TNA))
                                    factorK = tasa / cantidadPeriodosPago;
                                else
                                    factorK = Convert.ToDecimal(Math.Pow(Convert.ToDouble(1 + tasa), Convert.ToDouble(1m / cantidadPeriodosPago)) - 1.0);

                                valorCupon = instrumentoCuponeraDTO.ValorNominal * factorK / Convert.ToDecimal((1 - Math.Pow(Convert.ToDouble(1 + factorK), listaCupones.Count * (-1))));
                                valorCupon = Helper.TruncateDecimals(valorCupon, 7);
                                valorMontoAmortizacion = valorCupon - valorMontoIntereses;
                                break;
                            case (int)eTipoAmortizacionCertificadoDeposito.Variable:
                                //case (int)eTipoAmortizacionBonos.Variables:
                                valorMontoAmortizacion = decimal.Zero;
                                valorCupon = valorMontoAmortizacion + valorMontoIntereses;
                                break;
                            case (int)eTipoAmortizacionCertificadoDeposito.Vencimiento:
                                //case (int)eTipoAmortizacionBonos.Ninguno:
                                valorMontoAmortizacion = cuponGenerado.NumeroCupon.Equals(listaCupones.Count) ? instrumentoCuponeraDTO.ValorNominal : decimal.Zero;
                                //valorSaldoAmortizacion = cuponGenerado.NumeroCupon.Equals(listaCupones.Count) ? decimal.Zero : instrumentoCuponeraDTO.ValorNominal;
                                valorCupon = valorMontoAmortizacion + valorMontoIntereses;
                                break;
                        }

                        cuponGenerado.MontoAmortizacion = valorMontoAmortizacion;
                        cuponGenerado.MontoInteres = valorMontoIntereses;
                        cuponGenerado.MontoCupon = valorCupon;
                        cuponGenerado.SaldoPorAmortizar = valorSaldoAmortizacion - cuponGenerado.MontoAmortizacion;
                        cuponGenerado.FechaInicio = cuponGenerado.FechaInicioDate.ToString("dd/MM/yyyy");
                        cuponGenerado.FechaCorte = cuponGenerado.FechaCorteDate.ToString("dd/MM/yyyy");
                        cuponGenerado.FechaPago = cuponGenerado.FechaPagoDate.ToString("dd/MM/yyyy");
                    }
                    catch (OverflowException)
                    {
                        throw new ArgumentException("Los montos o valores ingresados para el cálculo de cuponera esta creando un desbordamiento de datos. Por favor asegurese de haber ingresado sus datos correctamente.");
                    }
                }

                return listaCupones.ToArray();
            }
        }


        public InstrumentoCuponeraListadoDTO[] GetPreviewCuponeraSOFR(InstrumentoCuponeraDTO instrumentoCuponeraDTO)
        {

            List<InstrumentoCuponeraListadoDTO> listaCupones = new List<InstrumentoCuponeraListadoDTO>();
            IndicadorDTO indicadorInteres = new IndicadorDTO();
            IndicadorDTO indicadorPeriodoGracia = new IndicadorDTO();
            IndicadorDTO indicadorTipoAmortizacion = new IndicadorDTO();
            IndicadorDTO indicadorBaseCalculo = new IndicadorDTO();
            IndicadorDTO indicadorPeriodoPago = new IndicadorDTO();
            IndicadorDTO indicadorIndexadoInflacion = new IndicadorDTO();

            MonedaDTO monedaIndexada = new MonedaDTO();
            int numeroCupon = 1;
            int indicadorTipoCalculoInteres = instrumentoCuponeraDTO.IdTipoCalculoInteres;
            int indicadorTipoInteres = instrumentoCuponeraDTO.IdTipoInteres;

            VerifyDatosCuponera(instrumentoCuponeraDTO, out indicadorInteres, out indicadorPeriodoGracia, out indicadorTipoAmortizacion, out indicadorBaseCalculo, out indicadorPeriodoPago, out monedaIndexada, out indicadorIndexadoInflacion);
            int cantidadMesesPeriodoPago = Convert.ToInt32(indicadorPeriodoPago.ValorAuxDecimal1 == null ? 0 : indicadorPeriodoPago.ValorAuxDecimal1.Value);
            int cantidadDiasPeriodoPago = Convert.ToInt32(indicadorPeriodoPago.ValorAuxNum1.Value);
            var IdiDTO = iIdiAppService.GetFirstIDIOpenWhenAllFondoPensionIsOpen();
            var fecha = Helper.ConvertIdFechaToDateTime(IdiDTO.IdSecuencialFechaIDI);
            var Stock = decimal.Zero;

            ////Validacion
            if (instrumentoCuponeraDTO.IdTipoCalculoInteres == Convert.ToInt32(eTipoCalculoInteres.ATasaVencida))
            {
                DateTime fechaCorteCuponera = Convert.ToDateTime(instrumentoCuponeraDTO.FechaCorteUltimoCupon);
                DateTime FechaInicioCuponera = Convert.ToDateTime(instrumentoCuponeraDTO.FechaInicioPrimerCupon);
                int diasCupon = (fechaCorteCuponera - FechaInicioCuponera).Days;
                int diasBucle = 0;
                DateTime fechaCalculoReal = DateTime.Now;
                DateTime fechaDiaAnterior;
                decimal valorTasaSOFRAnterior = 0;
                List<string> FechaTasaNula = new List<string>();
                for (int i = 0; i <= diasCupon; i = i + diasBucle)
                {
                    decimal valorTasaSOFR = 0;
                    fechaCalculoReal = Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaCorteCuponera));
                    fechaDiaAnterior = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteCuponera));
                    TimeSpan dias = (fechaCalculoReal - fechaDiaAnterior);
                    diasBucle = Convert.ToInt32(dias.TotalDays);
                    valorTasaSOFR = iTasaAppService.GetLastActiveTasaValorSOFRByIdTasaAndFecha(instrumentoCuponeraDTO.IdTasaLibor, fechaCalculoReal);
                    if (valorTasaSOFR == 0)
                    {
                        if (valorTasaSOFRAnterior != 0)
                        {
                            FechaTasaNula.Add(fechaCalculoReal.ToString("dd/MM/yyyy"));
                        }
                    }
                    fechaCorteCuponera = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteCuponera));
                    valorTasaSOFRAnterior = valorTasaSOFR;
                }

                if (FechaTasaNula.Count > 0)
                {
                    string diasSinTasa = null;
                    bool primeraTasa = true;
                    foreach (var fechaTasaNula in FechaTasaNula)
                    {
                        diasSinTasa = primeraTasa == true ? fechaTasaNula : ", " + fechaTasaNula;
                    }
                    throw new ArgumentException("Favor de registrar las siguientes tasas para las fechas: " + diasSinTasa);
                }
            }
            else //A tasa Adelantada
            {

                DateTime fechaCorteCuponera = Convert.ToDateTime(instrumentoCuponeraDTO.FechaCorteUltimoCupon).AddMonths(-cantidadMesesPeriodoPago * 1);
                DateTime FechaInicioCuponera = Convert.ToDateTime(instrumentoCuponeraDTO.FechaInicioPrimerCupon).AddMonths(-cantidadMesesPeriodoPago * 1).AddDays(-1);
                int diasCupon = (fechaCorteCuponera - FechaInicioCuponera).Days;
                int diasBucle = 0;
                DateTime fechaCalculoReal = DateTime.Now;
                DateTime fechaDiaAnterior;
                decimal valorTasaSOFRAnterior = 0;
                List<string> FechaTasaNula = new List<string>();
                for (int i = 0; i <= diasCupon; i = i + diasBucle)
                {
                    decimal valorTasaSOFR = 0;
                    fechaCalculoReal = Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaCorteCuponera));
                    fechaDiaAnterior = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteCuponera));
                    TimeSpan dias = (fechaCalculoReal - fechaDiaAnterior);
                    diasBucle = Convert.ToInt32(dias.TotalDays);
                    valorTasaSOFR = iTasaAppService.GetLastActiveTasaValorSOFRByIdTasaAndFecha(instrumentoCuponeraDTO.IdTasaLibor, fechaCalculoReal);
                    if (valorTasaSOFR == 0)
                    {
                        if (valorTasaSOFRAnterior != 0)
                        {
                            FechaTasaNula.Add(fechaCalculoReal.ToString("dd/MM/yyyy"));
                        }

                    }
                    fechaCorteCuponera = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteCuponera));
                    valorTasaSOFRAnterior = valorTasaSOFR;
                }

                if (FechaTasaNula.Count > 0)
                {
                    string diasSinTasa = null;
                    bool primeraTasa = true;
                    foreach (var fechaTasaNula in FechaTasaNula)
                    {
                        diasSinTasa = primeraTasa == true ? fechaTasaNula : ", " + fechaTasaNula;
                    }
                    throw new ArgumentException("Favor de registrar las siguientes tasas para las fechas: " + diasSinTasa);
                }
            }



            if (instrumentoCuponeraDTO.IdInstrumento != null && instrumentoCuponeraDTO.IdInstrumento != 0)
            {
                var dataStock = iInstrumentoStockRepository.GetFiltered(x => x.IdInstrumento == instrumentoCuponeraDTO.IdInstrumento && x.IdSecuencialFecha == IdiDTO.IdSecuencialFechaIDI).ToArray();
                if (dataStock.Any())
                    Stock = dataStock.Sum(x => x.SaldoContable);
            }


            if (indicadorPeriodoPago.IdIndicador.Equals((int)ePeriodoPago.Vencimiento) || indicadorPeriodoPago.IdIndicador.Equals((int)ePeriodoPagoCertificadoDeposito.Vencimiento))
            {
                InstrumentoCuponeraListadoDTO cupon = new InstrumentoCuponeraListadoDTO();
                cupon.NumeroCupon = numeroCupon;
                cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime;
                cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaVencimientoDateTime;
                cupon.FechaPagoDate = ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, cupon.FechaCorteDate);
                cupon.Dias = (cupon.FechaCorteDate - cupon.FechaInicioDate).Days;
                cupon.Indicador = cupon.FechaCorteDate >= fecha ? eEstadoVigenciaCupon.Vigente.ToString() : eEstadoVigenciaCupon.Vencido.ToString();
                cupon.IdIndicador = cupon.FechaCorteDate >= fecha ? (int)eEstadoVigenciaCupon.Vigente : (int)eEstadoVigenciaCupon.Vencido;
                cupon.Factor = monedaIndexada.IdMoneda == 0 ? decimal.Zero : ObtenerFactor(monedaIndexada.IdMoneda, cupon.FechaInicioDate, cupon.FechaCorteDate);
                cupon.EstadoCupon = eEstadoCupon.Calculado.ToString();
                cupon.IdIndicadorEstadoCupon = (int)eEstadoCupon.Calculado;
                listaCupones.Add(cupon);
            }
            else
            {
                //Calculamos los valores que pueden obtenerse sin saber la cantidad total de cupones
                while ((listaCupones.Count == 0 ? DateTime.MinValue : listaCupones.Last().FechaCorteDate) < instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime)
                {
                    bool calcularDiferenciaDias = false;
                    InstrumentoCuponeraListadoDTO cupon = new InstrumentoCuponeraListadoDTO();
                    cupon.NumeroCupon = numeroCupon;
                    if (instrumentoCuponeraDTO.MesesCalendario)
                    {

                        if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                        {
                            if (listaCupones.Count.Equals(0))
                                cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                            else
                                cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * listaCupones.Count).AddDays(1);
                            cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                        }
                        else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                        {
                            cupon.FechaInicioDate = listaCupones.Count == 0 ? instrumentoCuponeraDTO.FechaEmisionDateTime.AddDays(1) : listaCupones.Last().FechaCorteDate.AddDays(1);
                            cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime.AddMonths(cantidadMesesPeriodoPago * listaCupones.Count);
                        }
                    }
                    else if (instrumentoCuponeraDTO.PersonalizaDiasMes)
                    {

                        if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                        {
                            if (listaCupones.Count.Equals(0))
                            {
                                cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                                cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                            }
                            else
                            {
                                cupon.FechaInicioDate = listaCupones.Last().FechaCorteDate.AddDays(1);
                                cupon.FechaCorteDate = cupon.FechaInicioDate.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago).AddDays(-1);
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                        {
                            if (listaCupones.Count.Equals(0))
                            {
                                cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddDays(1);
                                cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime;
                            }
                            else
                            {
                                cupon.FechaInicioDate = listaCupones.Last().FechaCorteDate.AddDays(1);
                                cupon.FechaCorteDate = cupon.FechaInicioDate.AddDays(instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago);
                            }
                        }
                    }
                    else if (instrumentoCuponeraDTO.FinesMes)
                    {

                        if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
                        {
                            if (listaCupones.Count.Equals(0))
                                cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                            else
                                cupon.FechaInicioDate = listaCupones.Last().FechaCorteDate.AddDays(1);
                            cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                        }
                        else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                        {
                            //VERIFICAR ¿Como seria este caso?
                            if (listaCupones.Count.Equals(0))
                            {
                                cupon.FechaInicioDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddDays(1);
                                cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime;
                            }
                            else
                            {
                                cupon.FechaInicioDate = listaCupones.Last().FechaCorteDate.AddDays(1);
                                cupon.FechaCorteDate = instrumentoCuponeraDTO.FechaEmisionDateTime.AddMonths(cantidadMesesPeriodoPago * (listaCupones.Count + 1));
                            }
                        }
                        cupon.FechaCorteDate = new DateTime(cupon.FechaCorteDate.Year, cupon.FechaCorteDate.Month, 1).AddMonths(1).AddDays(-1);
                    }

                    //Verificamos que la fecha no exceda la fecha de corte del último cupón
                    DateTime fechaCorteEsperada = cupon.FechaCorteDate;
                    cupon.FechaCorteDate = cupon.FechaCorteDate > instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime ? instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime : cupon.FechaCorteDate;
                    cupon.FechaPagoDate = ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, cupon.FechaCorteDate);

                    if (instrumentoCuponeraDTO.PersonalizaDiasMes || instrumentoCuponeraDTO.FinesMes || instrumentoCuponeraDTO.EsBaseCalculoActual)
                        cupon.Dias = (cupon.FechaCorteDate - cupon.FechaInicioDate).Days + 1;
                    else if (instrumentoCuponeraDTO.MesesCalendario)
                    {
                        //En caso que se haya seleccionado la fecha de inicio o corte del primer cupon y sea el primer cupon
                        if (listaCupones.Count == 0 || (cupon.FechaCorteDate.Equals(instrumentoCuponeraDTO.FechaVencimientoDateTime) && !fechaCorteEsperada.Equals(cupon.FechaCorteDate)))
                            calcularDiferenciaDias = EsComportamientoBaseCalculoActual(instrumentoCuponeraDTO.FechaEmisionDateTime, cupon.FechaInicioDate, cupon.FechaCorteDate, instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime, instrumentoCuponeraDTO.FechaVencimientoDateTime, cantidadMesesPeriodoPago);
                        if (calcularDiferenciaDias)
                            cupon.Dias = (cupon.FechaCorteDate - cupon.FechaInicioDate).Days + 1;
                        else
                            cupon.Dias = instrumentoCuponeraDTO.PersonalizaDiasMes ? instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago : cantidadDiasPeriodoPago;
                    }
                    else
                        cupon.Dias = instrumentoCuponeraDTO.PersonalizaDiasMes ? instrumentoCuponeraDTO.CantidadDiasMes * cantidadMesesPeriodoPago : cantidadDiasPeriodoPago;

                    cupon.Indicador = Convert.ToDateTime(cupon.FechaCorteDate.ToString("dd/MM/yyyy")) >= Convert.ToDateTime(fecha.ToString("dd/MM/yyyy")) ? eEstadoVigenciaCupon.Vigente.ToString() : eEstadoVigenciaCupon.Vencido.ToString();
                    cupon.IdIndicador = Convert.ToDateTime(cupon.FechaCorteDate.ToString("dd/MM/yyyy")) >= Convert.ToDateTime(fecha.ToString("dd/MM/yyyy")) ? (int)eEstadoVigenciaCupon.Vigente : (int)eEstadoVigenciaCupon.Vencido;
                    cupon.Factor = monedaIndexada.IdMoneda == 0 ? 0 : ObtenerFactor(monedaIndexada.IdMoneda, instrumentoCuponeraDTO.FechaEmisionDateTime, cupon.FechaCorteDate);
                    cupon.EstadoCupon = eEstadoCupon.Calculado.ToString();
                    cupon.IdIndicadorEstadoCupon = (int)eEstadoCupon.Calculado;

                    if (cupon.FechaCorteDate.ConvertToIdFecha() == IdiDTO.IdSecuencialFechaIDI)
                    {
                        if (Stock == 0)
                        {
                            cupon.Indicador = eEstadoVigenciaCupon.Vencido.ToString();
                            cupon.IdIndicador = (int)eEstadoVigenciaCupon.Vencido;
                        }
                        else
                        {
                            cupon.Indicador = eEstadoVigenciaCupon.Vigente.ToString();
                            cupon.IdIndicador = (int)eEstadoVigenciaCupon.Vigente;
                        }
                    }
                    listaCupones.Add(cupon);
                    numeroCupon++;
                }
            }

            //Calculamos todos los montos de cupon, amortizacion, intereses y saldo de amortización
            decimal valorMontoAmortizacion = decimal.Zero;
            decimal valorMontoIntereses = decimal.Zero;
            decimal valorCupon = decimal.Zero;
            decimal valorSaldoAmortizacion = decimal.Zero;
            decimal interesAcumulado = decimal.Zero;
            bool primerRegistroInteresCompuesto = true;
            decimal diasTotales = decimal.Zero;
            decimal TasaSinRound = decimal.Zero;
            int numeroCuponAplicaCalculoInteres = indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Interes) ? listaCupones.Find(lc => lc.FechaInicioDate <= instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime && lc.FechaCorteDate >= instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime).NumeroCupon + 1 : 0;
            int numeroCuponAplicaCalculoAmortizacion = indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Amortizacion) ? listaCupones.Find(lc => lc.FechaInicioDate <= instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime && lc.FechaCorteDate >= instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime).NumeroCupon + 1 : 0;


            foreach (InstrumentoCuponeraListadoDTO cuponGenerado in listaCupones)
            {
                try
                {
                    valorSaldoAmortizacion = cuponGenerado.NumeroCupon.Equals(1) ? instrumentoCuponeraDTO.ValorNominalVigente : listaCupones[cuponGenerado.NumeroCupon - 2].SaldoPorAmortizar;
                    if (monedaIndexada.IdMoneda != 0 && indicadorIndexadoInflacion.IdIndicador.Equals((int)eIndexadoInflacion.AlPrincipal))
                        cuponGenerado.ValorNominalVigenteIndexado = Math.Round((valorSaldoAmortizacion * cuponGenerado.Factor), 7);

                    if ((indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Interes) && cuponGenerado.NumeroCupon < numeroCuponAplicaCalculoInteres) || (instrumentoCuponeraDTO.EsBonoLetraHipotecaria && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.CeroCupon)))
                        valorMontoIntereses = decimal.Zero;
                    else
                    {
                        decimal valorBaseCalculo = ObtenerBaseCalculoCuponera(indicadorBaseCalculo, cuponGenerado.FechaInicioDate, cuponGenerado.FechaCorteDate, cuponGenerado.Dias);
                        decimal valorCalculoInteres = monedaIndexada.IdMoneda == 0 ? valorSaldoAmortizacion : cuponGenerado.ValorNominalVigenteIndexado;


                        if (instrumentoCuponeraDTO.IdIndicadorIntereses == Convert.ToInt32(eIndicadorInteresesBonos.TasaFlotante))
                        {
                            if (instrumentoCuponeraDTO.IdTipoInteres == Convert.ToInt32(eTipoInteres.Simple))
                            {
                                if (instrumentoCuponeraDTO.IdTipoCalculoInteres == Convert.ToInt32(eTipoCalculoInteres.ATasaAdelantada))
                                {
                                    //Tasa Adelantada Simple
                                    DateTime fechaCorteEsperada = cuponGenerado.FechaCorteDate;
                                    DateTime fechaCalculoReal = DateTime.Now;
                                    DateTime fechaDiaAnterior;
                                    Boolean primerCuponSOFR = false;
                                    Decimal montoInteresCupon = 0;
                                    Decimal principalInteresAcumuladoAnterior = 0;

                                    DateTime fechaCorteEsperada1 = cuponGenerado.FechaCorteDate.AddMonths(-cantidadMesesPeriodoPago * 1);
                                    DateTime FechaInicioDate2 = cuponGenerado.FechaInicioDate.AddMonths(-cantidadMesesPeriodoPago * 1).AddDays(-1);
                                    int Dias1 = (FechaInicioDate2 - fechaCorteEsperada1).Days * -1;
                                    fechaCorteEsperada = fechaCorteEsperada1;
                                    DateTime fechaCalculoReal1 = DateTime.Now;
                                    int DiasAnteriores = 0;
                                    int diasBucle = 0;
                                    int diasCupon = cuponGenerado.Dias;
                                    List<InstrumentoCuponeraListadoSOFRDTO> listaCuponesSOFR = new List<InstrumentoCuponeraListadoSOFRDTO>();

                                    for (int i = 0; i <= Dias1; i = i + diasBucle)
                                    {
                                        InstrumentoCuponeraListadoSOFRDTO cuponSFOR = new InstrumentoCuponeraListadoSOFRDTO();
                                        decimal valorTasaSOFR = 0;
                                        fechaCalculoReal = Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        fechaDiaAnterior = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));//Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaDiaAnterior));
                                        cuponSFOR.Fecha = fechaCalculoReal;
                                        TimeSpan dias = (fechaCalculoReal - fechaDiaAnterior);
                                        cuponSFOR.Dias = Convert.ToInt32(dias.TotalDays);
                                        diasBucle = cuponSFOR.Dias;
                                        valorTasaSOFR = iTasaAppService.GetLastActiveTasaValorSOFRByIdTasaAndFecha(instrumentoCuponeraDTO.IdTasaLibor, fechaCalculoReal);
                                        cuponSFOR.TasaAnulizada = valorTasaSOFR;
                                        cuponSFOR.TasaEfectiva = primerCuponSOFR == false ? 0 : (cuponSFOR.TasaAnulizada / 100 * DiasAnteriores / Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1));
                                        cuponSFOR.Principal = valorCalculoInteres;
                                        principalInteresAcumuladoAnterior = primerCuponSOFR == false ? valorCalculoInteres : montoInteresCupon;
                                        cuponSFOR.CargoInteres = Math.Round(valorCalculoInteres * valorTasaSOFR, 2);
                                        cuponSFOR.PrincipalInteresAcumulado = primerCuponSOFR == false ? valorCalculoInteres : valorCalculoInteres + cuponSFOR.CargoInteres;
                                        montoInteresCupon = cuponSFOR.PrincipalInteresAcumulado;
                                        primerCuponSOFR = true;
                                        DiasAnteriores = cuponSFOR.Dias;
                                        fechaCorteEsperada = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        listaCuponesSOFR.Add(cuponSFOR);
                                    }


                                    decimal Suma = 0;
                                    decimal Producto = 0;
                                    decimal ProductoMenos1 = 0;
                                    decimal tasaSimpleAnual = 0;
                                    decimal tasaTotalSpread = 0;
                                    diasTotales = 0;
                                    foreach (var item in listaCuponesSOFR)
                                    {
                                        if (item.TasaEfectiva == 0)
                                        {
                                            ProductoMenos1 = 0;
                                        }
                                        else
                                        {
                                            Suma = Suma + item.TasaEfectiva;

                                        }
                                    }
                                    tasaSimpleAnual = Suma * Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1) / Dias1;
                                    //tasaTotalSpread = tasaSimpleAnual * 100 + instrumentoCuponeraDTO.TasaVariableCupon;
                                    //cuponGenerado.Tasa = tasaTotalSpread;
                                    TasaSinRound = (tasaSimpleAnual * 100 + instrumentoCuponeraDTO.TasaVariableCupon);
                                    cuponGenerado.Tasa = Math.Round(TasaSinRound, 7);
                                    valorMontoIntereses = montoInteresCupon;
                                }
                                else
                                {
                                    //Tasa Vencida Simple
                                    DateTime fechaCorteEsperada = cuponGenerado.FechaCorteDate;
                                    DateTime fechaCalculoReal = DateTime.Now;
                                    DateTime fechaDiaAnterior;
                                    Boolean primerCuponSOFR = false;
                                    Decimal montoInteresCupon = 0;
                                    Decimal principalInteresAcumuladoAnterior = 0;
                                    int DiasAnteriores = 0;
                                    int diasBucle = 0;
                                    List<InstrumentoCuponeraListadoSOFRDTO> listaCuponesSOFR = new List<InstrumentoCuponeraListadoSOFRDTO>();

                                    for (int i = 0; i <= cuponGenerado.Dias; i = i + diasBucle)
                                    {
                                        InstrumentoCuponeraListadoSOFRDTO cuponSFOR = new InstrumentoCuponeraListadoSOFRDTO();
                                        decimal valorTasaSOFR = 0;
                                        fechaCalculoReal = Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        fechaDiaAnterior = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));//Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaDiaAnterior));
                                        cuponSFOR.Fecha = fechaCalculoReal;
                                        TimeSpan dias = (fechaCalculoReal - fechaDiaAnterior);
                                        cuponSFOR.Dias = Convert.ToInt32(dias.TotalDays);
                                        diasBucle = cuponSFOR.Dias;
                                        valorTasaSOFR = iTasaAppService.GetLastActiveTasaValorSOFRByIdTasaAndFecha(instrumentoCuponeraDTO.IdTasaLibor, fechaCalculoReal);
                                        cuponSFOR.TasaAnulizada = valorTasaSOFR;
                                        cuponSFOR.TasaEfectiva = primerCuponSOFR == false ? 0 : (cuponSFOR.TasaAnulizada / 100 * DiasAnteriores / Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1));
                                        cuponSFOR.Principal = valorCalculoInteres;
                                        principalInteresAcumuladoAnterior = primerCuponSOFR == false ? valorCalculoInteres : montoInteresCupon;
                                        cuponSFOR.CargoInteres = Math.Round(valorCalculoInteres * valorTasaSOFR, 2);
                                        cuponSFOR.PrincipalInteresAcumulado = primerCuponSOFR == false ? valorCalculoInteres : valorCalculoInteres + cuponSFOR.CargoInteres;
                                        montoInteresCupon = cuponSFOR.PrincipalInteresAcumulado;
                                        primerCuponSOFR = true;
                                        DiasAnteriores = cuponSFOR.Dias;
                                        fechaCorteEsperada = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        listaCuponesSOFR.Add(cuponSFOR);
                                    }


                                    decimal Suma = 0;
                                    decimal tasaSimpleAnual = 0;
                                    decimal tasaTotalSpread = 0;
                                    diasTotales = 0;
                                    foreach (var item in listaCuponesSOFR)
                                    {
                                        if (item.TasaEfectiva == 0)
                                        {
                                            Suma = 0;
                                        }
                                        else
                                        {
                                            Suma = Suma + item.TasaEfectiva;
                                        }
                                    }
                                    tasaSimpleAnual = Suma * Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1) / cuponGenerado.Dias;
                                    //tasaTotalSpread = tasaSimpleAnual*100 + instrumentoCuponeraDTO.TasaVariableCupon;
                                    //cuponGenerado.Tasa = tasaTotalSpread;
                                    TasaSinRound = (tasaSimpleAnual * 100 + instrumentoCuponeraDTO.TasaVariableCupon);
                                    cuponGenerado.Tasa = Math.Round(TasaSinRound, 7);
                                    valorMontoIntereses = montoInteresCupon;

                                }
                            }
                            else
                            {
                                //TASA COMPUESTA
                                if (instrumentoCuponeraDTO.IdTipoCalculoInteres == Convert.ToInt32(eTipoCalculoInteres.ATasaAdelantada))
                                {
                                    DateTime fechaCorteEsperada = cuponGenerado.FechaCorteDate;
                                    DateTime fechaCalculoReal = DateTime.Now;
                                    DateTime fechaDiaAnterior;
                                    Boolean primerCuponSOFR = false;
                                    Decimal montoInteresCupon = 0;
                                    Decimal principalInteresAcumuladoAnterior = 0;

                                    DateTime fechaCorteEsperada1 = cuponGenerado.FechaCorteDate.AddMonths(-cantidadMesesPeriodoPago * 1);
                                    DateTime FechaInicioDate2 = cuponGenerado.FechaInicioDate.AddMonths(-cantidadMesesPeriodoPago * 1).AddDays(-1);
                                    int Dias1 = (FechaInicioDate2 - fechaCorteEsperada1).Days * -1;
                                    fechaCorteEsperada = fechaCorteEsperada1;
                                    DateTime fechaCalculoReal1 = DateTime.Now;
                                    int DiasAnteriores = 0;
                                    int diasBucle = 0;
                                    int diasCupon = cuponGenerado.Dias;
                                    List<InstrumentoCuponeraListadoSOFRDTO> listaCuponesSOFR = new List<InstrumentoCuponeraListadoSOFRDTO>();

                                    for (int i = 0; i <= Dias1; i = i + diasBucle)
                                    {
                                        InstrumentoCuponeraListadoSOFRDTO cuponSFOR = new InstrumentoCuponeraListadoSOFRDTO();
                                        decimal valorTasaSOFR = 0;
                                        fechaCalculoReal = Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        fechaDiaAnterior = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));//Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaDiaAnterior));
                                        cuponSFOR.Fecha = fechaCalculoReal;
                                        TimeSpan dias = (fechaCalculoReal - fechaDiaAnterior);
                                        cuponSFOR.Dias = Convert.ToInt32(dias.TotalDays);
                                        diasBucle = cuponSFOR.Dias;
                                        valorTasaSOFR = iTasaAppService.GetLastActiveTasaValorSOFRByIdTasaAndFecha(instrumentoCuponeraDTO.IdTasaLibor, fechaCalculoReal);
                                        cuponSFOR.TasaAnulizada = valorTasaSOFR;
                                        cuponSFOR.TasaEfectiva = primerCuponSOFR == false ? 0 : (1 + (cuponSFOR.TasaAnulizada / 100 * DiasAnteriores / Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1)));
                                        cuponSFOR.Principal = valorCalculoInteres;
                                        principalInteresAcumuladoAnterior = primerCuponSOFR == false ? valorCalculoInteres : montoInteresCupon;
                                        cuponSFOR.CargoInteres = Math.Round(valorCalculoInteres * valorTasaSOFR, 2);
                                        cuponSFOR.PrincipalInteresAcumulado = primerCuponSOFR == false ? valorCalculoInteres : valorCalculoInteres + cuponSFOR.CargoInteres;
                                        montoInteresCupon = cuponSFOR.PrincipalInteresAcumulado;
                                        primerCuponSOFR = true;
                                        DiasAnteriores = cuponSFOR.Dias;
                                        fechaCorteEsperada = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        listaCuponesSOFR.Add(cuponSFOR);
                                    }

                                    decimal Producto = 0;
                                    decimal ProductoMenos1 = 0;
                                    decimal tasaCompuestaAnual = 0;
                                    foreach (var item in listaCuponesSOFR)
                                    {
                                        if (item.TasaEfectiva == 0)
                                        {
                                            ProductoMenos1 = 0;
                                        }
                                        else
                                        {
                                            Producto = Producto == 0 ? item.TasaEfectiva : (Producto * item.TasaEfectiva);
                                        }
                                    }
                                    ProductoMenos1 = Producto - 1;
                                    tasaCompuestaAnual = ProductoMenos1 * Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1) / Dias1;
                                    TasaSinRound = (tasaCompuestaAnual * 100 + instrumentoCuponeraDTO.TasaVariableCupon);
                                    cuponGenerado.Tasa = Math.Round(TasaSinRound, 7);
                                    valorMontoIntereses = montoInteresCupon;
                                }
                                else
                                {
                                    //TASA COMPUESTA VENCIDA
                                    DateTime fechaCorteEsperada = cuponGenerado.FechaCorteDate;
                                    DateTime fechaCalculoReal = DateTime.Now;
                                    DateTime fechaDiaAnterior;
                                    Boolean primerCuponSOFR = false;
                                    Decimal montoInteresCupon = 0;
                                    Decimal principalInteresAcumuladoAnterior = 0;
                                    int DiasAnteriores = 0;
                                    int diasBucle = 0;
                                    int diasCupon = cuponGenerado.Dias;
                                    List<InstrumentoCuponeraListadoSOFRDTO> listaCuponesSOFR = new List<InstrumentoCuponeraListadoSOFRDTO>();

                                    for (int i = 0; i <= cuponGenerado.Dias; i = i + diasBucle)
                                    {
                                        InstrumentoCuponeraListadoSOFRDTO cuponSFOR = new InstrumentoCuponeraListadoSOFRDTO();
                                        decimal valorTasaSOFR = 0;
                                        fechaCalculoReal = Convert.ToDateTime(ObtenerFechaPagoCupon(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        fechaDiaAnterior = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        cuponSFOR.Fecha = fechaCalculoReal;
                                        TimeSpan dias = (fechaCalculoReal - fechaDiaAnterior);
                                        cuponSFOR.Dias = Convert.ToInt32(dias.TotalDays);
                                        diasBucle = cuponSFOR.Dias;
                                        valorTasaSOFR = iTasaAppService.GetLastActiveTasaValorSOFRByIdTasaAndFecha(instrumentoCuponeraDTO.IdTasaLibor, fechaCalculoReal);
                                        cuponSFOR.TasaAnulizada = valorTasaSOFR;
                                        cuponSFOR.TasaEfectiva = primerCuponSOFR == false ? 0 : (1 + (cuponSFOR.TasaAnulizada / 100 * DiasAnteriores / Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1)));
                                        cuponSFOR.Principal = valorCalculoInteres;
                                        principalInteresAcumuladoAnterior = primerCuponSOFR == false ? valorCalculoInteres : montoInteresCupon;
                                        cuponSFOR.CargoInteres = Math.Round(valorCalculoInteres * valorTasaSOFR, 2);
                                        cuponSFOR.PrincipalInteresAcumulado = primerCuponSOFR == false ? valorCalculoInteres : valorCalculoInteres + cuponSFOR.CargoInteres;
                                        montoInteresCupon = cuponSFOR.PrincipalInteresAcumulado;
                                        primerCuponSOFR = true;
                                        DiasAnteriores = cuponSFOR.Dias;
                                        fechaCorteEsperada = Convert.ToDateTime(ObtenerFechaAnteriorLaborable(instrumentoCuponeraDTO.IdMercado, fechaCorteEsperada));
                                        listaCuponesSOFR.Add(cuponSFOR);
                                    }


                                    decimal Producto = 0;
                                    decimal tasaCompuestaAnual = 0;
                                    foreach (var item in listaCuponesSOFR)
                                    {
                                        if (item.TasaEfectiva == 0)
                                        {
                                            Producto = 0;
                                        }
                                        else
                                        {
                                            Producto = Producto == 0 ? item.TasaEfectiva : (Producto * item.TasaEfectiva);
                                        }
                                    }
                                    tasaCompuestaAnual = (Producto - 1) * Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1) / cuponGenerado.Dias;
                                    TasaSinRound = (tasaCompuestaAnual * 100 + instrumentoCuponeraDTO.TasaVariableCupon);
                                    cuponGenerado.Tasa = Math.Round(TasaSinRound, 7);
                                    valorMontoIntereses = montoInteresCupon;
                                }


                            }
                        }
                        else
                        {
                            if (instrumentoCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TNA))
                                valorMontoIntereses = valorCalculoInteres * (cuponGenerado.Tasa / 100) * valorBaseCalculo;
                            else if (instrumentoCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TEA))
                                valorMontoIntereses = Convert.ToDecimal(Convert.ToDouble(valorCalculoInteres) * (Math.Pow((1 + Convert.ToDouble(cuponGenerado.Tasa / 100)), Convert.ToDouble(valorBaseCalculo)) - 1));
                        }



                        valorMontoIntereses = Helper.TruncateDecimals(valorMontoIntereses, 7);
                    }

                    switch (indicadorTipoAmortizacion.IdIndicador)
                    {
                        case (int)eTipoAmortizacionCertificadoDeposito.Constantes:
                            //case (int)eTipoAmortizacionBonos.Constante:
                            if (indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Amortizacion) && cuponGenerado.NumeroCupon < numeroCuponAplicaCalculoAmortizacion)
                                valorMontoAmortizacion = decimal.Zero;
                            else
                                valorMontoAmortizacion = instrumentoCuponeraDTO.ValorNominal / listaCupones.FindAll(lc => lc.NumeroCupon >= numeroCuponAplicaCalculoAmortizacion).Count();
                            //Se agrego la condicion para que el ultimo cupon coja el saldo por amoritzar del cupon anterior.
                            if (cuponGenerado.NumeroCupon.Equals(listaCupones.Count))
                                valorMontoAmortizacion = listaCupones.FindAll(lc => lc.NumeroCupon.Equals(cuponGenerado.NumeroCupon - 1)).First().SaldoPorAmortizar;

                            valorMontoAmortizacion = Helper.TruncateDecimals(valorMontoAmortizacion, 7);
                            valorCupon = valorMontoAmortizacion + valorMontoIntereses;
                            break;
                        case (int)eTipoAmortizacionCertificadoDeposito.CuotasIguales:
                            //case (int)eTipoAmortizacionBonos.Cuotas
                            decimal factorK = decimal.Zero;
                            decimal tasa = instrumentoCuponeraDTO.TasaFijaCupon.Equals(0) ? instrumentoCuponeraDTO.TasaVariableCupon : instrumentoCuponeraDTO.TasaFijaCupon;
                            decimal cantidadPeriodosPago = cantidadMesesPeriodoPago >= 1 ? Convert.ToDecimal(12 / cantidadMesesPeriodoPago) : Convert.ToDecimal(12 / 0.5);
                            tasa = (tasa / 100);
                            if (instrumentoCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TNA))
                                factorK = tasa / cantidadPeriodosPago;
                            else
                                factorK = Convert.ToDecimal(Math.Pow(Convert.ToDouble(1 + tasa), Convert.ToDouble(1m / cantidadPeriodosPago)) - 1.0);

                            valorCupon = instrumentoCuponeraDTO.ValorNominal * factorK / Convert.ToDecimal((1 - Math.Pow(Convert.ToDouble(1 + factorK), listaCupones.Count * (-1))));
                            valorCupon = Helper.TruncateDecimals(valorCupon, 7);
                            valorMontoAmortizacion = valorCupon - valorMontoIntereses;
                            break;
                        case (int)eTipoAmortizacionCertificadoDeposito.Variable:
                            //case (int)eTipoAmortizacionBonos.Variables:
                            valorMontoAmortizacion = decimal.Zero;
                            valorCupon = valorMontoAmortizacion + valorMontoIntereses;
                            break;
                        case (int)eTipoAmortizacionCertificadoDeposito.Vencimiento:
                            //case (int)eTipoAmortizacionBonos.Ninguno:
                            valorMontoAmortizacion = cuponGenerado.NumeroCupon.Equals(listaCupones.Count) ? instrumentoCuponeraDTO.ValorNominal : decimal.Zero;
                            //valorCupon = Math.Round((cuponGenerado.Tasa / 100) * instrumentoCuponeraDTO.ValorNominal * cuponGenerado.Dias / Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1),7);
                            valorCupon = Math.Round((TasaSinRound / 100) * instrumentoCuponeraDTO.ValorNominal * cuponGenerado.Dias / Convert.ToDecimal(indicadorBaseCalculo.ValorAuxNum1), 7);


                            break;
                    }

                    cuponGenerado.MontoAmortizacion = valorMontoAmortizacion;
                    cuponGenerado.MontoInteres = valorCupon;
                    cuponGenerado.MontoCupon = valorCupon;
                    cuponGenerado.SaldoPorAmortizar = valorSaldoAmortizacion - cuponGenerado.MontoAmortizacion;
                    cuponGenerado.FechaInicio = cuponGenerado.FechaInicioDate.ToString("dd/MM/yyyy");
                    cuponGenerado.FechaCorte = cuponGenerado.FechaCorteDate.ToString("dd/MM/yyyy");
                    cuponGenerado.FechaPago = cuponGenerado.FechaPagoDate.ToString("dd/MM/yyyy");
                }
                catch (OverflowException)
                {
                    throw new ArgumentException("Los montos o valores ingresados para el cálculo de cuponera esta creando un desbordamiento de datos. Por favor asegurese de haber ingresado sus datos correctamente.");
                }
            }

            return listaCupones.ToArray();
        }

        bool EsComportamientoBaseCalculoActual(DateTime fechaEmision, DateTime fechaInicioCupon, DateTime fechaCorteCupon, DateTime fechaCorteUltimoCupon, DateTime fechaVencimiento, int cantidadMesesPeriodoPago)
        {
            bool calcularDiferenciaDias = false;
            DateTime fechaCorteCuponAnterior = fechaInicioCupon.AddDays(-1);
            if (fechaCorteCuponAnterior.Day.Equals(fechaCorteCupon.Day))
            {
                int diferenciaMeses = fechaCorteCupon.Month - fechaCorteCuponAnterior.Month + 12 * (fechaCorteCupon.Year - fechaCorteCuponAnterior.Year);
                if (diferenciaMeses != cantidadMesesPeriodoPago)
                    calcularDiferenciaDias = true;
            }
            else
                calcularDiferenciaDias = true;
            return calcularDiferenciaDias;
        }

        public InstrumentoCuponeraListadoDTO[] RecalcularCuponera(InstrumentoCuponeraListadoDTO[] cuponeraDTO, InstrumentoCuponeraDTO configuracionCuponeraDTO)
        {
            IndicadorDTO indicadorPeriodoGracia = new IndicadorDTO();
            IndicadorDTO indicadorTipoAmortizacion = new IndicadorDTO();
            IndicadorDTO indicadorBaseCalculo = new IndicadorDTO();
            MonedaDTO monedaIndexada = new MonedaDTO();
            VerifyDatosRecalcularCuponera(configuracionCuponeraDTO, out indicadorPeriodoGracia, out indicadorTipoAmortizacion, out indicadorBaseCalculo, out monedaIndexada);

            List<InstrumentoCuponeraListadoDTO> listaCuponesRecalculada = new List<InstrumentoCuponeraListadoDTO>();
            InstrumentoCuponeraListadoDTO cuponModificado = cuponeraDTO.First(c => c.CuponModificado);
            //Establecemos los valores del cupon modificado
            decimal valorNominalVigente = cuponModificado.NumeroCupon.Equals(1) ? configuracionCuponeraDTO.ValorNominal : cuponeraDTO.First(c => c.NumeroCupon.Equals(cuponModificado.NumeroCupon - 1)).SaldoPorAmortizar;
            decimal valorNominalVigenteIndexado = decimal.Zero;
            if (monedaIndexada.IdMoneda != 0)
            {
                //decimal irdFechaEmision = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(monedaIndexada.IdMoneda, cuponModificado.FechaInicioDate);
                //decimal irdFechaConsulta = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(monedaIndexada.IdMoneda, cuponModificado.FechaCorteDate);
                valorNominalVigenteIndexado = Math.Round((valorNominalVigente * cuponModificado.Factor), 7);
            }
            decimal valorBaseCalculoCuponModificado = ObtenerBaseCalculoCuponera(indicadorBaseCalculo, cuponModificado.FechaInicioDate, cuponModificado.FechaCorteDate, cuponModificado.Dias);
            decimal valorCalculoInteres = monedaIndexada.IdMoneda == 0 ? valorNominalVigente : valorNominalVigenteIndexado;
            if (configuracionCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TNA))
                cuponModificado.MontoInteres = valorCalculoInteres * (cuponModificado.Tasa / 100) * valorBaseCalculoCuponModificado;
            else if (configuracionCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TEA))
                cuponModificado.MontoInteres = Convert.ToDecimal(Convert.ToDouble(valorCalculoInteres) * (Math.Pow((1 + Convert.ToDouble(cuponModificado.Tasa / 100)), Convert.ToDouble(valorBaseCalculoCuponModificado)) - 1));

            cuponModificado.MontoCupon = cuponModificado.MontoAmortizacion + cuponModificado.MontoInteres;
            cuponModificado.SaldoPorAmortizar = valorNominalVigente - cuponModificado.MontoAmortizacion;

            //Establecemos los valores de los siguientes cupones hasta el ultimo
            int numeroUltimoCupon = cuponeraDTO.Max(c => c.NumeroCupon);
            decimal valorMontoAmortizacion = decimal.Zero;
            decimal valorMontoIntereses = decimal.Zero;
            decimal valorCupon = decimal.Zero;
            decimal valorSaldoAmortizacion = decimal.Zero;

            for (int numeroCupon = cuponModificado.NumeroCupon + 1; numeroCupon <= numeroUltimoCupon; numeroCupon++)
            {
                InstrumentoCuponeraListadoDTO cuponModificar = cuponeraDTO.First(c => c.NumeroCupon.Equals(numeroCupon));
                valorNominalVigente = cuponeraDTO.First(c => c.NumeroCupon.Equals(numeroCupon - 1)).SaldoPorAmortizar;
                if (monedaIndexada.IdMoneda != 0)
                {
                    //decimal irdFechaEmision = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(monedaIndexada.IdMoneda, cuponModificar.FechaInicioDate);
                    //decimal irdFechaConsulta = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(monedaIndexada.IdMoneda, cuponModificar.FechaCorteDate);
                    valorNominalVigenteIndexado = Math.Round((valorNominalVigente * cuponModificar.Factor), 7);
                }

                decimal valorBaseCalculo = ObtenerBaseCalculoCuponera(indicadorBaseCalculo, cuponModificar.FechaInicioDate, cuponModificar.FechaCorteDate, cuponModificar.Dias);
                valorCalculoInteres = monedaIndexada.IdMoneda == 0 ? valorNominalVigente : valorNominalVigenteIndexado;
                if (configuracionCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TNA))
                    valorMontoIntereses = valorCalculoInteres * (cuponModificar.Tasa / 100) * valorBaseCalculo;
                else if (configuracionCuponeraDTO.IdIndicadorTipoTasa.Equals((int)eTipoTasaAnual.TEA))
                    valorMontoIntereses = Convert.ToDecimal(Convert.ToDouble(valorCalculoInteres) * (Math.Pow((1 + Convert.ToDouble(cuponModificar.Tasa / 100)), Convert.ToDouble(valorBaseCalculo)) - 1));

                switch (indicadorTipoAmortizacion.IdIndicador)
                {
                    case (int)eTipoAmortizacionCertificadoDeposito.Constantes:
                    case (int)eTipoAmortizacionCertificadoDeposito.Variable:
                        valorMontoAmortizacion = cuponModificar.MontoAmortizacion;
                        valorCupon = valorMontoAmortizacion + valorMontoIntereses;
                        valorSaldoAmortizacion = valorNominalVigente - valorMontoAmortizacion;
                        break;
                    case (int)eTipoAmortizacionCertificadoDeposito.CuotasIguales:
                        valorCupon = cuponModificar.MontoCupon;
                        valorMontoAmortizacion = valorCupon - valorMontoIntereses;
                        valorSaldoAmortizacion = valorNominalVigente - valorMontoAmortizacion;
                        break;
                    case (int)eTipoAmortizacionCertificadoDeposito.Vencimiento:
                        valorMontoAmortizacion = numeroCupon.Equals(numeroUltimoCupon) ? valorNominalVigente : cuponModificar.MontoAmortizacion;
                        valorCupon = valorMontoAmortizacion + valorMontoIntereses;
                        valorSaldoAmortizacion = valorNominalVigente - valorMontoAmortizacion;
                        break;
                }

                cuponModificar.MontoAmortizacion = Math.Round(valorMontoAmortizacion, 7);
                cuponModificar.MontoInteres = Math.Round(valorMontoIntereses, 7);
                cuponModificar.MontoCupon = Math.Round(valorCupon, 7);
                cuponModificar.SaldoPorAmortizar = Math.Round(valorNominalVigente - valorMontoAmortizacion, 7);
            }

            cuponModificado.CuponModificado = false;
            cuponeraDTO = cuponeraDTO.OrderBy(x => x.NumeroCupon).ToArray();
            return cuponeraDTO.ToArray();
        }

        public bool EliminarCuponeraRentaFija(int idRentaFija)
        {
            int indEstadoVigenciaCuponVencido = iIndicadorAppService.GetId((int)eIndicador.EstadoVigenciaCupon, (int)eEstadoVigenciaCupon.Vencido);
            InstrumentoRentaFija instrumento = iInstrumentoRentaFijaRepository.Get(idRentaFija);
            InstrumentoRentaFijaCupon cuponVencido = instrumento.RentaFijaCupon.FirstOrDefault(rfc => rfc.IndEstadoVigenciaCupon.Equals(indEstadoVigenciaCuponVencido));
            if (cuponVencido == null)
                return true;
            else
                throw new ArgumentException("No se puede eliminar la cuponera del instrumento debido a que ya cuenta con cupones ya vencidos.");
        }

        public bool EliminarCuponeraCertificadoDeposito(int idCertificadoDepositoCortoPlazo)
        {
            int indEstadoVigenciaCuponVencido = iIndicadorAppService.GetId((int)eIndicador.EstadoVigenciaCupon, (int)eEstadoVigenciaCupon.Vencido);
            InstrumentoCertificadoDepositoCortoPlazo instrumento = iInstrumentoCertificadoDepositoCortoPlazoRepository.Get(idCertificadoDepositoCortoPlazo);
            InstrumentoCertificadoDepositoCortoPlazoCupon cuponVencido = instrumento.CertificadoDepositoCortoPlazoCupon.FirstOrDefault(cdc => cdc.IndEstadoVigenciaCupon.Equals(indEstadoVigenciaCuponVencido));
            if (cuponVencido == null)
                return true;
            else
                throw new ArgumentException("No se puede eliminar la cuponera del instrumento debido a que ya cuenta con cupones ya vencidos.");
        }

        void VerifyDatosCuponera(InstrumentoCuponeraDTO instrumentoCuponeraDTO, out IndicadorDTO indicadorInteres, out IndicadorDTO indicadorPeriodoGracia, out IndicadorDTO indicadorTipoAmortizacion, out IndicadorDTO indicadorBaseCalculo, out IndicadorDTO indicadorPeriodoPago, out MonedaDTO monedaIndexada, out IndicadorDTO indicadorIndexadoInflaxion)
        {
            //1.- Validamos las fechas ingresadas en la configuración de cuponera
            if (string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon) && string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                throw new ArgumentException("Definir si el cálculo es por fecha de inicio o corte para el 1er cupón.");
            else if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon) && !string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
                throw new ArgumentException("No se puede enviar la fecha de inicio del primer cupón y la fecha de corte del primer cupón a la vez.");

            //3.- Validamos el mercado.
            if (instrumentoCuponeraDTO.IdMercado == 0)
                throw new ArgumentException("No se ha seleccionado el Mercado necesario para la definición del calendario.");

            //4.- Validamos que si escogio la fecha de inicio del primer cupon, esta no sea menor que la fecha de emisión.
            if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon))
            {
                instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime = Convert.ToDateTime(instrumentoCuponeraDTO.FechaInicioPrimerCupon);
                if (instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime <= instrumentoCuponeraDTO.FechaEmisionDateTime)
                    throw new ArgumentException("La fecha de inicio del primer cupón no puede ser menor o igual a la fecha de emisión del instrumento.");
                if (instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime > instrumentoCuponeraDTO.FechaVencimientoDateTime)
                    throw new ArgumentException("La fecha de inicio del primer cupón no puede ser mayor a la fecha de vencimiento del instrumento.");
            }

            //5.- Validamos si escogio la fecha de corte del primer cupón.
            if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon))
            {
                instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime = Convert.ToDateTime(instrumentoCuponeraDTO.FechaCortePrimerCupon);
                if (instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime <= instrumentoCuponeraDTO.FechaEmisionDateTime)
                    throw new ArgumentException("La fecha de corte del primer cupón no puede ser menor o igual a la fecha de emisión del instrumento.");
                if (instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime > instrumentoCuponeraDTO.FechaVencimientoDateTime)
                    throw new ArgumentException("La fecha de corte del primer cupón no puede ser mayor a la fecha de vencimiento del instrumento.");
            }

            //6.- Validamos la fecha de corte del último cupón del popup de configuración
            if (string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCorteUltimoCupon))
                throw new ArgumentException("No se ha seleccionado la fecha de corte del último cupón.");
            else
            {
                instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime = Convert.ToDateTime(instrumentoCuponeraDTO.FechaCorteUltimoCupon);
                if (instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime <= instrumentoCuponeraDTO.FechaEmisionDateTime)
                    throw new ArgumentException("La fecha de corte del último cupón no puede ser menor o igual a la fecha de emisión del instrumento.");
                if (instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime > instrumentoCuponeraDTO.FechaVencimientoDateTime)
                    throw new ArgumentException("La fecha de corte del último cupón no puede ser mayor a la fecha de vencimiento del instrumento.");
                if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon) && instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime <= instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime)
                    throw new ArgumentException("La fecha de corte del último cupón no puede ser menor o igual a la fecha de inicio del primer cupón.");
                if (!string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCortePrimerCupon) && instrumentoCuponeraDTO.FechaCorteUltimoCuponDateTime < instrumentoCuponeraDTO.FechaCortePrimerCuponDateTime)
                    throw new ArgumentException("La fecha de corte del último cupón no puede ser menor a la fecha de corte del primer cupón.");
            }

            //7.- Validamos la moneda indexada y el indicador indexado inflación dependiendo del indicador de intereses.
            indicadorInteres = new IndicadorDTO();
            monedaIndexada = new MonedaDTO();
            indicadorIndexadoInflaxion = new IndicadorDTO();
            if (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo)
                indicadorInteres = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.IndicadorInteresesCertificadoDeposito, instrumentoCuponeraDTO.IdIndicadorIntereses);
            else if (instrumentoCuponeraDTO.EsBonoLetraHipotecaria)
                indicadorInteres = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.IndicadorInteresesBonos, instrumentoCuponeraDTO.IdIndicadorIntereses);

            if ((instrumentoCuponeraDTO.EsBonoLetraHipotecaria && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.IndexadoInflacion)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.IndexadoInflacion)))
            {
                monedaIndexada = iMonedaAppService.GetMonedaIndexada(instrumentoCuponeraDTO.IdMoneda);
                if (monedaIndexada == null)
                    throw new ArgumentException("La Moneda del instrumento no cuenta con una Moneda Indexada vigente y habilitada necesario para el cálculo del Factor.");

                indicadorIndexadoInflaxion = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IndIndexarInflacion);
                if (indicadorIndexadoInflaxion == null)
                    throw new ArgumentException("El indicador indexado inflación no ha sido seleccionado.");
            }

            //2.- Validamos el indicador de interes.
            if (instrumentoCuponeraDTO.IdIndicadorIntereses == 0 && ((instrumentoCuponeraDTO.EsBonoLetraHipotecaria && !indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && !indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.CeroCupon))))
                throw new ArgumentException("Definir el tipo de tasa de interes.");

            //8.- Validamos el periodo de gracia
            indicadorPeriodoGracia = new IndicadorDTO();
            if (instrumentoCuponeraDTO.IdPeriodoGracia > 0)
            {
                if (string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaFinPeriodoGracia))
                    throw new ArgumentException("Debe seleccionar la fecha de fin de periodo de gracia.");
                else
                {
                    indicadorPeriodoGracia = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IdPeriodoGracia);
                    instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime = Convert.ToDateTime(instrumentoCuponeraDTO.FechaFinPeriodoGracia);
                    if (instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime >= instrumentoCuponeraDTO.FechaVencimientoDateTime)
                        throw new ArgumentException("La fecha fin de periodo de gracia no puede ser mayor o igual a la fecha de vencimiento del instrumento.");
                    if (instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime <= instrumentoCuponeraDTO.FechaEmisionDateTime)
                        throw new ArgumentException("La fecha fin de periodo de gracia no puede ser menor o igual a la fecha de emisión del instrumento.");
                    if (indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Ambos))
                        instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime = instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime.AddDays(1);
                }
            }

            //9.- Validamos el tipo de amortizacion
            indicadorTipoAmortizacion = new IndicadorDTO();
            if (instrumentoCuponeraDTO.IdTipoAmortizacion == 0)
                throw new ArgumentException("No se ha seleccionado el tipo de amortización para la generación de la cuponera.");
            else
                indicadorTipoAmortizacion = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IdTipoAmortizacion);

            //10.- Validamos la base de calculo
            indicadorBaseCalculo = new IndicadorDTO();
            if (instrumentoCuponeraDTO.IdBaseCalculo == 0 && ((instrumentoCuponeraDTO.EsBonoLetraHipotecaria && !indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && !indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.CeroCupon))))
                throw new ArgumentException("No se ha seleccionado la base de cálculo para la generación de la cuponera.");
            else
            {
                if (instrumentoCuponeraDTO.IdBaseCalculo > 0)
                {
                    indicadorBaseCalculo = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IdBaseCalculo);
                    if (
                        (instrumentoCuponeraDTO.EsBonoLetraHipotecaria && (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_360) || indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_365) || indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_Actual)))
                        ||
                        (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoCDCP.Actual_360) || indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoCDCP.Actual_365) || indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoCDCP.Actual_Actual)))
                       )
                        instrumentoCuponeraDTO.EsBaseCalculoActual = true;
                }
            }

            //11.- Establecemos el valor nominal vigente
            instrumentoCuponeraDTO.ValorNominalVigente = instrumentoCuponeraDTO.ValorNominal;
            if (monedaIndexada.IdMoneda != 0)
            {
                decimal irdFechaEmision = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(monedaIndexada.IdMoneda, instrumentoCuponeraDTO.FechaEmisionDateTime);
                decimal irdFechaConsulta = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(monedaIndexada.IdMoneda, DateTime.Now);
                instrumentoCuponeraDTO.ValorNominalVigente = Math.Round((instrumentoCuponeraDTO.ValorNominal * irdFechaConsulta / irdFechaEmision), 7);
            }

            //12.- Validamos el periodo de pago
            indicadorPeriodoPago = new IndicadorDTO();
            if (instrumentoCuponeraDTO.IdPeriodoPago == 0)
                throw new ArgumentException("No se ha seleccionado el periodo de pago para la generación de la cuponera.");
            else
            {
                indicadorPeriodoPago = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IdPeriodoPago);
                //Verificamos que el periodo de pago ademas tenga correctamente configurado sus valores obligatorios de cuponera
                if ((!indicadorPeriodoPago.ValorAuxNum1.HasValue || !indicadorPeriodoPago.ValorAuxDecimal1.HasValue) && !indicadorPeriodoPago.IdIndicador.Equals((int)ePeriodoPago.Vencimiento) && ((instrumentoCuponeraDTO.EsBonoLetraHipotecaria && !indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && !indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.CeroCupon))))
                    throw new ArgumentException("El periodo de pago seleccionado no cuenta con la configuracion de dias necesaria para el calculo de cuponera.");
            }

            //13.- Fecha de camtio de tipo de tasa
            if (!string.IsNullOrEmpty(instrumentoCuponeraDTO.OrdenCambioTasa))
            {
                if (string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaCambioTipoTasa))
                    throw new ArgumentException("Debe seleccionar una fecha de tipo de cambio de tasa cuando selecciona el indicador de interes en Fija/Variable o Variable/Fija.");
                else
                {
                    instrumentoCuponeraDTO.FechaCambioTipoTasaDateTime = Convert.ToDateTime(instrumentoCuponeraDTO.FechaCambioTipoTasa);
                    if (instrumentoCuponeraDTO.FechaCambioTipoTasaDateTime <= instrumentoCuponeraDTO.FechaEmisionDateTime)
                        throw new ArgumentException("La fecha de cambio de tasa no puede ser menor o igual a la fecha de emisión del instrumento.");
                    if (instrumentoCuponeraDTO.FechaCambioTipoTasaDateTime > instrumentoCuponeraDTO.FechaVencimientoDateTime)
                        throw new ArgumentException("La fecha de cambio de tasa no puede ser mayor a la fecha de vencimiento del instrumento.");
                }
            }

            //14.- Verificar que la cantidad de dias del periodo de pago es menor que la diferencia de dias entre la fecha de vencimiento y emision del instrumento
            if (!indicadorPeriodoPago.IdIndicador.Equals((int)ePeriodoPago.Vencimiento))// && ((instrumentoCuponeraDTO.EsBonoLetraHipotecaria && !indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.CeroCupon)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && !indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.CeroCupon))))
            {
                DateTime fechaInicio = string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaInicioPrimerCupon) ? instrumentoCuponeraDTO.FechaEmisionDateTime : instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime;
                DateTime fechaFin = instrumentoCuponeraDTO.FechaVencimientoDateTime;
                if (indicadorPeriodoPago.ValorAuxNum1.Value > ((fechaFin - fechaInicio).Days + 1))
                    throw new ArgumentException("La cantidad de dias del periodo de pago no puede ser mayor a la cantidad de dias desde la fecha de inicio del primer cupón y la fecha de corte del último cupón.");
            }
        }

        void VerifyDatosRecalcularCuponera(InstrumentoCuponeraDTO instrumentoCuponeraDTO, out IndicadorDTO indicadorPeriodoGracia, out IndicadorDTO indicadorTipoAmortizacion, out IndicadorDTO indicadorBaseCalculo, out MonedaDTO monedaIndexada)
        {
            indicadorPeriodoGracia = new IndicadorDTO();
            if (instrumentoCuponeraDTO.IdPeriodoGracia > 0)
            {
                if (string.IsNullOrWhiteSpace(instrumentoCuponeraDTO.FechaFinPeriodoGracia))
                    throw new ArgumentException("Debe seleccionar la fecha de fin de periodo de gracia.");
                else
                {
                    indicadorPeriodoGracia = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IdPeriodoGracia);
                    instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime = Convert.ToDateTime(instrumentoCuponeraDTO.FechaFinPeriodoGracia);
                    if (instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime >= instrumentoCuponeraDTO.FechaVencimientoDateTime)
                        throw new ArgumentException("La fecha fin de periodo de gracia no puede ser mayor o igual a la fecha de vencimiento del instrumento.");
                    if (instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime <= instrumentoCuponeraDTO.FechaEmisionDateTime)
                        throw new ArgumentException("La fecha fin de periodo de gracia no puede ser menor o igual a la fecha de emisión del instrumento.");
                    if (indicadorPeriodoGracia.IdIndicador.Equals((int)ePeriodoDeGracia.Ambos))
                        instrumentoCuponeraDTO.FechaInicioPrimerCuponDateTime = instrumentoCuponeraDTO.FechaFinPeriodoGraciaDateTime.AddDays(1);
                }
            }

            IndicadorDTO indicadorInteres = new IndicadorDTO();
            IndicadorDTO indicadorIndexadoInflaxion = new IndicadorDTO();
            monedaIndexada = new MonedaDTO();
            if (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo)
                indicadorInteres = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.IndicadorInteresesCertificadoDeposito, instrumentoCuponeraDTO.IdIndicadorIntereses);
            else if (instrumentoCuponeraDTO.EsBonoLetraHipotecaria)
                indicadorInteres = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.IndicadorInteresesBonos, instrumentoCuponeraDTO.IdIndicadorIntereses);

            if ((instrumentoCuponeraDTO.EsBonoLetraHipotecaria && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesBonos.IndexadoInflacion)) || (instrumentoCuponeraDTO.EsCertificadoDepositoCortoPlazo && indicadorInteres.IdIndicador.Equals((int)eIndicadorInteresesCertificadoDeposito.IndexadoInflacion)))
            {
                monedaIndexada = iMonedaAppService.GetMonedaIndexada(instrumentoCuponeraDTO.IdMoneda);
                if (monedaIndexada == null)
                    throw new ArgumentException("La Moneda del instrumento no cuenta con una Moneda Indexada vigente y habilitada necesario para el cálculo del Factor.");

                indicadorIndexadoInflaxion = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IndIndexarInflacion);
                if (indicadorIndexadoInflaxion == null)
                    throw new ArgumentException("El indicador indexado inflación no ha sido seleccionado.");
            }

            indicadorTipoAmortizacion = new IndicadorDTO();
            if (instrumentoCuponeraDTO.IdTipoAmortizacion == 0)
                throw new ArgumentException("No se ha seleccionado el tipo de amortización para la generación de la cuponera.");
            else
                indicadorTipoAmortizacion = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IdTipoAmortizacion);

            indicadorBaseCalculo = new IndicadorDTO();
            if (instrumentoCuponeraDTO.IdBaseCalculo == 0)
                throw new ArgumentException("No se ha seleccionado la base de cálculo para la generación de la cuponera.");
            else
                indicadorBaseCalculo = iIndicadorAppService.GetById(instrumentoCuponeraDTO.IdBaseCalculo);
        }

        InstrumentoCuponeraListadoDTO[] GenerarCeroCupon(string fechaEmision, string fechaVencimiento, decimal valorNominal, int idMercado)
        {
            //throw new ArgumentException("No se puede generar la cuponera para el instrumento debido a que no se ha seleccionado un mercado.");
            return new List<InstrumentoCuponeraListadoDTO>(){
            new InstrumentoCuponeraListadoDTO(){
            NumeroCupon = 1,
            FechaInicio = fechaEmision,
            FechaCorte = fechaVencimiento,
            FechaPago = ObtenerFechaPagoCupon(idMercado, Convert.ToDateTime(fechaVencimiento)).ToString("dd/MM/yyyy"),
            Dias = 0,
            Indicador = eEstadoVigenciaCupon.Vigente.ToString(),
            IdIndicador = (int)eEstadoVigenciaCupon.Vigente,
            Tasa = decimal.Zero,
            Factor = 1,
            MontoCupon = valorNominal,
            MontoInteres = decimal.Zero,
            MontoAmortizacion = valorNominal,
            SaldoPorAmortizar = decimal.Zero,
            IdIndicadorEstadoCupon = (int)eEstadoCupon.Calculado
            }}.ToArray();
        }

        private DateTime ObtenerFechaCorteCupon(eTipoCorteFechaPago tipoCorteFechaPago, DateTime fechaInicio, int idPeriodoPago, bool esPrimerCupon = false, int? diaMesPrimerCupon = null, int? diasPorMes = null)
        {
            Indicador indicadorPeriodoPago = iIndicadorAppService.GetIndicador(idPeriodoPago);
            int cantidadDiasPeriodoPago = indicadorPeriodoPago.ValorAuxNum1.Value;
            decimal cantidadMesesPeriodoPago = indicadorPeriodoPago.ValorAuxDecimal1.Value;

            DateTime fechaCorteCupon = fechaInicio;
            switch (tipoCorteFechaPago)
            {
                case eTipoCorteFechaPago.MesesCalendario:
                    fechaCorteCupon = fechaInicio.AddDays(cantidadDiasPeriodoPago);
                    if (!esPrimerCupon && !fechaInicio.Day.Equals(diaMesPrimerCupon.Value))
                        fechaCorteCupon = new DateTime(fechaCorteCupon.Year, fechaCorteCupon.Month, diaMesPrimerCupon.Value);
                    break;
                case eTipoCorteFechaPago.PersonalizaDiasMes:
                    fechaCorteCupon = fechaInicio.AddDays(diasPorMes.Value * Convert.ToInt32(cantidadMesesPeriodoPago));
                    break;
                case eTipoCorteFechaPago.FinesMes:
                    fechaCorteCupon = fechaInicio.AddDays(cantidadDiasPeriodoPago);
                    fechaCorteCupon = new DateTime(fechaCorteCupon.Year, fechaCorteCupon.Month, 1).AddMonths(1).AddDays(-1);
                    break;
            }

            return fechaCorteCupon;
        }

        private DateTime ObtenerFechaPagoCupon(int idMercado, DateTime fechaCorteCupon)
        {
            string fechaCorte = fechaCorteCupon.ToString("dd/MM/yyyy");
            string fechaPago = iDiasNoLaborablesAppService.GetSiguienteDiaLaborable(idMercado, fechaCorte);
            DateTime fechaPagoCupon = Convert.ToDateTime(fechaPago);
            return fechaPagoCupon;
        }

        private DateTime ObtenerFechaAnteriorLaborable(int idMercado, DateTime fechaCorteCupon)
        {
            string fechaCorte = fechaCorteCupon.ToString("dd/MM/yyyy");
            string fechaPago = iDiasNoLaborablesAppService.GetAnteriorDiaLaborable(idMercado, fechaCorte);
            DateTime fechaPagoCupon = Convert.ToDateTime(fechaPago);
            return fechaPagoCupon;
        }

        private decimal ObtenerTasaCupon(string ordenCambioTasa, decimal tasaFijaCupon, int idTasaLibor, decimal tasaVariableCupon, DateTime fechaCambioTipoTasa, DateTime fechaInicioCupon, IndicadorDTO indicadorIndexadoInflacion, int idMonedaIndexada, DateTime fechaCorteCupon)
        {
            const string tipoFijaVariable = "FV";

            decimal tasaCupon = decimal.Zero;

            if (string.IsNullOrWhiteSpace(ordenCambioTasa) && idTasaLibor == 0)
                tasaCupon = tasaFijaCupon;
            else if (string.IsNullOrWhiteSpace(ordenCambioTasa) && idTasaLibor > 0)
            {
                decimal valorTasaLibor = iTasaAppService.GetLastActiveTasaValorByIdTasaAndFecha(idTasaLibor, fechaInicioCupon);
                tasaCupon = valorTasaLibor + tasaVariableCupon;
            }
            else
            {
                if (ordenCambioTasa != null && ordenCambioTasa.ToUpper().Trim().Equals(tipoFijaVariable))
                {
                    if (fechaInicioCupon < fechaCambioTipoTasa)
                        tasaCupon = tasaFijaCupon;
                    else
                    {
                        decimal valorTasaLibor = iTasaAppService.GetLastActiveTasaValorByIdTasaAndFecha(idTasaLibor, fechaInicioCupon);
                        tasaCupon = valorTasaLibor + tasaVariableCupon;
                    }
                }
                else
                {
                    if (fechaInicioCupon < fechaCambioTipoTasa)
                    {
                        decimal valorTasaLibor = iTasaAppService.GetLastActiveTasaValorByIdTasaAndFecha(idTasaLibor, fechaInicioCupon);
                        tasaCupon = valorTasaLibor + tasaVariableCupon;
                    }
                    else
                        tasaCupon = tasaFijaCupon;
                }
            }
            //Verificamos si tiene el indicador de interes es Indexacion
            if (indicadorIndexadoInflacion.IdIndicador.Equals((int)eIndexadoInflacion.AlCupon))
            {
                decimal irdFechaCorteCupon = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(idMonedaIndexada, fechaCorteCupon);
                decimal irdFechaInicioCupon = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(idMonedaIndexada, fechaInicioCupon);
                tasaCupon = tasaCupon + (irdFechaCorteCupon / irdFechaInicioCupon);
            }
            return Math.Round(tasaCupon, 7);
        }

        public decimal ObtenerBaseCalculo(int idBaseCalculo, DateTime fechaInicioCupon, DateTime fechaCorteCupon, DateTime fechaVencimiento, out int diasDevengados)
        {
            decimal baseCalculo = decimal.Zero;

            IndicadorDTO indicadorBaseCalculo = iIndicadorAppService.GetById(idBaseCalculo);
            int ultimoDiaFebreroFechaInicio = new DateTime(fechaInicioCupon.Year, 2, 1).AddMonths(1).AddDays(-1).Day;
            int ultimoDiaFebreroFechaCorte = new DateTime(fechaCorteCupon.Year, 2, 1).AddMonths(1).AddDays(-1).Day;
            int numeroDiasDiferenciaNumerador = 0;
            int numeroDiasDiferenciaDenominador = 0;
            #region Calculo Numerador

            if
                (
                 (
                  (
                   indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30_360) ||
                   indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30E_360) ||
                   indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30_365)
                  ) &&
                  fechaInicioCupon.Day.Equals(31)
                 ) ||
                 (
                  indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30E_360_ISDA) &&
                  (
                   (!fechaInicioCupon.Month.Equals(2) && fechaInicioCupon.Day.Equals(31)) ||
                   (fechaInicioCupon.Month.Equals(2) && fechaInicioCupon.Day.Equals(ultimoDiaFebreroFechaInicio))
                  )
                 )
                )
            {
                if (!fechaInicioCupon.Month.Equals(2))
                    fechaInicioCupon = new DateTime(fechaInicioCupon.Year, fechaInicioCupon.Month, 30);
                else
                {
                    numeroDiasDiferenciaNumerador = 30 - ultimoDiaFebreroFechaInicio;
                    //fechaCorteCupon = new DateTime(fechaCorteCupon.Year, fechaCorteCupon.Month, ultimoDiaFebreroFechaCorte).AddDays(numeroDiasDiferencia);
                }
            }

            if
                (
                 (
                  (
                   indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30_360) ||
                   indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30_365)
                  ) &&
                  fechaCorteCupon.Day.Equals(31) &&
                  (fechaInicioCupon.Day.Equals(30) || fechaInicioCupon.Day.Equals(31))
                 ) ||
                 (
                  indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30E_360) && fechaCorteCupon.Day.Equals(31)
                 ) ||
                 (
                  indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30E_360_ISDA) &&
                  (
                   (!fechaCorteCupon.Month.Equals(2) && fechaCorteCupon.Day.Equals(31)) ||
                   (fechaCorteCupon.Month.Equals(2) && fechaCorteCupon.Day.Equals(ultimoDiaFebreroFechaCorte) && !fechaCorteCupon.Equals(fechaVencimiento))
                  )
                 )
                )
            {
                if (!fechaCorteCupon.Month.Equals(2))
                    fechaCorteCupon = new DateTime(fechaCorteCupon.Year, fechaCorteCupon.Month, 30);
                else
                {
                    numeroDiasDiferenciaDenominador = 30 - ultimoDiaFebreroFechaCorte;
                    //fechaCorteCupon = new DateTime(fechaCorteCupon.Year, fechaCorteCupon.Month, ultimoDiaFebreroFechaCorte).AddDays(numeroDiasDiferencia);
                }
            }


            #endregion

            #region Calculo Denominador

            decimal denominadorBaseCalculo = 0;
            if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30_360) ||
                indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30E_360) ||
                indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30E_360_ISDA) ||
                indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_360))
                denominadorBaseCalculo = 360m;
            else if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30_365) ||
                     indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_365))
                denominadorBaseCalculo = 365m;
            else if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_Actual)) //Cantidad de dias del año de inicio
                denominadorBaseCalculo = Convert.ToDecimal((new DateTime(fechaInicioCupon.Year, 1, 1) - new DateTime(fechaInicioCupon.Year, 12, 31)).Days);

            #endregion

            //Si es actual sobre algo, solo es la diferencia
            if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_360) || indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_365) || indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_Actual))
                diasDevengados = (fechaCorteCupon - fechaInicioCupon).Days;
            else
                diasDevengados = (360 * (fechaCorteCupon.Year - fechaInicioCupon.Year)) + (30 * (fechaCorteCupon.Month - fechaInicioCupon.Month)) + (fechaCorteCupon.Day - fechaInicioCupon.Day) + numeroDiasDiferenciaNumerador + numeroDiasDiferenciaDenominador;
            baseCalculo = Convert.ToDecimal(diasDevengados) / denominadorBaseCalculo;
            return baseCalculo;
        }

        public decimal ObtenerBaseCalculoCuponera(IndicadorDTO indicadorBaseCalculo, DateTime fechaInicioCupon, DateTime fechaCorteCupon, int dias)
        {
            decimal valorBaseCalculo = decimal.Zero;
            decimal valorDias = Convert.ToDecimal(dias);
            if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30_360) || indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30E_360) || indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30E_360_ISDA))
                valorBaseCalculo = valorDias / 360;
            else if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.v30_365))
                valorBaseCalculo = valorDias / 365;
            else if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_360))
                valorBaseCalculo = Convert.ToDecimal((fechaCorteCupon - fechaInicioCupon).Days + 1) / 360;
            else if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_365))
                valorBaseCalculo = Convert.ToDecimal((fechaCorteCupon - fechaInicioCupon).Days + 1) / 365;
            else if (indicadorBaseCalculo.IdIndicador.Equals((int)eBaseCalculoBono.Actual_Actual))
            {
                int numeroDias = (fechaCorteCupon - fechaInicioCupon).Days;
                int cantidadDiasAnio = (new DateTime(fechaInicioCupon.Year, 12, 31) - new DateTime(fechaInicioCupon.Year, 1, 1)).Days + 1;
                valorBaseCalculo = Convert.ToDecimal(numeroDias) / Convert.ToDecimal(cantidadDiasAnio);
            }
            return valorBaseCalculo;
        }

        private decimal ObtenerFactor(int idMonedaIndexada, DateTime fechaInicioCupon, DateTime fechaCorteCupon)
        {
            decimal factor = decimal.Zero;
            decimal valorFechaInicio = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(idMonedaIndexada, fechaInicioCupon);
            decimal valorFechaCorte = iIndiceIndexacionMonedaAppService.GetIndiceIndexacionMonedaByFecha(idMonedaIndexada, fechaCorteCupon);
            factor = Math.Round(valorFechaCorte / valorFechaInicio, 7);
            return factor;
        }

        private enum eTipoCorteFechaPago
        {
            MesesCalendario,
            PersonalizaDiasMes,
            FinesMes
        }
        #endregion

        public InteresDevengadoListadoPagedDTO GetFilteredDataInteresesDevengados(InteresDevengadoFilterDTO interesDevengadoFilter)
        {
            int idSecuencialFechaInicio = 0;
            int idSecuencialFechaFin = 0;
            int idSecuencialFechaConsulta = 0;
            if (!string.IsNullOrWhiteSpace(interesDevengadoFilter.FechaInicio))
                idSecuencialFechaInicio = Helper.ConvertFechaStringToIdFecha(interesDevengadoFilter.FechaInicio);
            if (!string.IsNullOrWhiteSpace(interesDevengadoFilter.FechaFin))
                idSecuencialFechaFin = Helper.ConvertFechaStringToIdFecha(interesDevengadoFilter.FechaFin);
            if (!string.IsNullOrWhiteSpace(interesDevengadoFilter.FechaConsulta))
                idSecuencialFechaConsulta = Helper.ConvertFechaStringToIdFecha(interesDevengadoFilter.FechaConsulta);

            InteresDevengadoListadoPagedDTO listadoInteresesDevengados = iInstrumentoDataRepository.GetFilteredDataInteresesDevengados(
                interesDevengadoFilter.CurrentPage,
                interesDevengadoFilter.ItemsPerPage,
                interesDevengadoFilter.OrderColumn,
                interesDevengadoFilter.IsAscending,
                idSecuencialFechaInicio,
                idSecuencialFechaFin,
                idSecuencialFechaConsulta,
                interesDevengadoFilter.Nemotecnico,
                interesDevengadoFilter.IdMoneda,
                interesDevengadoFilter.IdFondo
            );
            /*
            for (int i = 0; i < listadoInteresesDevengados.InteresesDevengados.Length; i++)
            {
                InteresDevengadoListadoDTO interesDevengadoDTO = new InteresDevengadoListadoDTO();
                var item = listadoInteresesDevengados.InteresesDevengados[i];
                if (idSecuencialFechaInicio > 0)
                    interesDevengadoDTO = CalcularInteresesDevengados(item.IdInstrumento, Convert.ToDateTime(interesDevengadoFilter.FechaInicio), Convert.ToDateTime(interesDevengadoFilter.FechaFin), item.IdFondo, item.CantidadContable);
                else
                    interesDevengadoDTO = CalcularInteresesDevengados(item.IdInstrumento, null, Convert.ToDateTime(interesDevengadoFilter.FechaConsulta), item.IdFondo, item.CantidadContable);
                item.InteresesDevengados = interesDevengadoDTO.InteresesDevengados;

                item.DiasDevengados = interesDevengadoDTO.DiasDevengados;
            }*/
            /*
        
            if (interesDevengadoFilter.OrderColumn != null)
            {
                if (interesDevengadoFilter.IsAscending)
                {
                    if (interesDevengadoFilter.OrderColumn.Equals("InteresesDevengados", StringComparison.InvariantCultureIgnoreCase))
                        listadoInteresesDevengados.InteresesDevengados = listadoInteresesDevengados.InteresesDevengados.OrderBy(x => x.InteresesDevengados).ToArray();
                    if (interesDevengadoFilter.OrderColumn.Equals("DiasDevengados", StringComparison.InvariantCultureIgnoreCase))
                        listadoInteresesDevengados.InteresesDevengados = listadoInteresesDevengados.InteresesDevengados.OrderBy(x => x.DiasDevengados).ToArray();
                }
                else
                {
                    if (interesDevengadoFilter.OrderColumn.Equals("InteresesDevengados", StringComparison.InvariantCultureIgnoreCase))
                        listadoInteresesDevengados.InteresesDevengados = listadoInteresesDevengados.InteresesDevengados.OrderByDescending(x => x.InteresesDevengados).ToArray();
                    if (interesDevengadoFilter.OrderColumn.Equals("DiasDevengados", StringComparison.InvariantCultureIgnoreCase))
                        listadoInteresesDevengados.InteresesDevengados = listadoInteresesDevengados.InteresesDevengados.OrderByDescending(x => x.DiasDevengados).ToArray();
                }
            }*/
            return listadoInteresesDevengados;

        }

        public string[] GetAllCodigoSbsInstrumento()
        {
            return iInstrumentoRepository.GetAll().OrderBy(i => i.CodigoSbs).Select(i => i.CodigoSbs).ToArray();
        }

        public InstrumentoListadoReporteDTO[] GetAllInstrumentoForFilterReporte(InstrumentoFilterReporteDTO instrumentoFilterReporte)
        {
            return iInstrumentoDataRepository.GetAllInstrumentoForFilterReporte(instrumentoFilterReporte).ToArray();
        }
        public InstrumentoListadoReporteDTO[] GetAllInstrumentosByFiltersReporte(InstrumentoFilterReporteDTO instrumentoFilterReporte)
        {
            return iInstrumentoDataRepository.GetAllInstrumentosByFiltersReporte(instrumentoFilterReporte).ToArray();
        }

        public InstrumentoListadoDTO[] GetAllInstrumentos()
        {
            return iInstrumentoDataRepository.GetAllInstrumentos();
        }

        public InstrumentoListadoDTO[] GetAllInstrumentosVigentesYHabilitados()
        {
            return iInstrumentoDataRepository.GetAllInstrumentosVigentesYHabilitados();
        }

        public InstrumentoNemotecnico[] getAllNemotecnico()
        {
            var Insturmentos = iInstrumentoRepository.getAllViewInstrumento();
            return Insturmentos.Select(x => new InstrumentoNemotecnico
            {
                IdInstrumento = x.IdInstrumento,
                Nemotecnico = x.Nemotecnico
            }).ToArray();
        }

        public InstrumentoNemotecnico[] getAllNemotecnicoRentaFija()
        {
            return iInstrumentoRepository.GetAllRentaFija().Select(x => new InstrumentoNemotecnico
            {
                IdInstrumento = x.IdInstrumento,
                Nemotecnico = x.Nemotecnico
            }).ToArray();
            //var Insturmentos = iInstrumentoRepository.getAllViewInstrumento();
            //var grupoInstrumento = iGrupoInstrumentoAppService.GetAllForRentaFija();

            //return Insturmentos
            //    .Where(x => grupoInstrumento.Any(y => y.IdGrupoInstrumento == x.IdGrupoInstrumento))
            //    .Select(x => new InstrumentoNemotecnico
            //{
            //    IdInstrumento = x.IdInstrumento,
            //    Nemotecnico = x.Nemotecnico
            //}).ToArray();
        }

        public InstrumentoNemotecnico[] getAllNemotecnicoRentaVariable()
        {
            return iInstrumentoRepository.GetAllRentaVariable().Select(x => new InstrumentoNemotecnico
            {
                IdInstrumento = x.IdInstrumento,
                Nemotecnico = x.Nemotecnico
            }).ToArray();

            //var Insturmentos = iInstrumentoRepository.getAllViewInstrumento();
            //var grupoInstrumento = iGrupoInstrumentoAppService.GetAllForRentaVariable();

            //return Insturmentos
            //    .Where(x => grupoInstrumento.Any(y => y.IdGrupoInstrumento == x.IdGrupoInstrumento))
            //    .Select(x => new InstrumentoNemotecnico
            //    {
            //        IdInstrumento = x.IdInstrumento,
            //        Nemotecnico = x.Nemotecnico
            //    }).ToArray();
        }

        public InstrumentoAlternativoMontoComprometidoDTO[] GetMontosComprometidosByIdFondoAlternativo(int idFondoAlternativo)
        {
            var instrumento = iInstrumentoFondoAlternativoRepository.Get(idFondoAlternativo);
            var fondosComprometidos = instrumento.InstrumentoFondoAlternativoMontoComprometido.ToArray();
            return fondosComprometidos.Select(x => new InstrumentoAlternativoMontoComprometidoDTO
            {
                Fondo = x.FondoPension.NombreFondo,
                IdFondo = x.IdFondoPension,
                MontoComprometidoActual = x.MontoComprometidoActual,
                MontoComprometidoInicial = x.MontoComprometidoInicial,
                CodigoFondo = x.FondoPension.CodigoFondo
            }).ToArray();
        }

        #region Private Methods

        string GeneratedCodigoSbsOnlyFirst7Digits(string codigoSbsIncomplete, int IdMoneda, int IdEmisor, int idTipoInstrumento, int tipo)
        {
            string codigoSbsComplete = "";

            if (codigoSbsIncomplete.Length == (int)CustomLength.CodeSBSInstrumento)
                return codigoSbsIncomplete;

            //var nacionalidad = iIndicadorRepository.FirstOrDefault(x => x.Id == IndNacionalidad);
            var TipoInstrumento = iTipoInstrumentoRepository.Get(idTipoInstrumento);
            if (TipoInstrumento == null)
                throw new Exception(String.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "Tipo Instrumento"));

            var moneda = iMonedaAppService.GetAllActiveHabilitado().FirstOrDefault(x => x.IdMoneda == IdMoneda);
            if (moneda == null)
                throw new Exception(String.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "Moneda vigente y habilitada"));

            var entidad = iEntidadAppService.GetActiveAllByRol((int)eRol.RolEmisor).FirstOrDefault(x => x.IdEntidad == IdEmisor);
            if (entidad == null)
                throw new Exception(String.Format(mensajeGenericoES.ERROR_PARAMETRO_NULO, "Entidad vigente con rol emisor"));

            if (tipo == 1)      //accion      
                codigoSbsComplete = String.Concat(String.Concat(String.Concat(TipoInstrumento.CodigoSbsTipoInstrumento, entidad.CodigoSbsEmisor), moneda.CodigoSBS), codigoSbsIncomplete);
            else if (tipo != 1)
                codigoSbsComplete = String.Concat(String.Concat(String.Concat(TipoInstrumento.CodigoSbsTipoInstrumento, entidad.CodigoSbsEmisor), moneda.CodigoSBS), codigoSbsIncomplete);

            return codigoSbsComplete;
        }

        void AddNewRentaFijaCupon(InstrumentoCuponeraListadoDTO cupon, int idRentaFija, string loginActualizacion)
        {
            InstrumentoRentaFijaCupon cuponRentaFija = new InstrumentoRentaFijaCupon();
            cuponRentaFija.IdRentaFija = idRentaFija;
            cuponRentaFija.NumeroCupon = cupon.NumeroCupon;
            cuponRentaFija.FechaInicio = Convert.ToDateTime(cupon.FechaInicio);
            cuponRentaFija.FechaCorte = Convert.ToDateTime(cupon.FechaCorte);
            cuponRentaFija.FechaPago = Convert.ToDateTime(cupon.FechaPago);
            cuponRentaFija.IndEstadoVigenciaCupon = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.EstadoVigenciaCupon, cupon.IdIndicador).Id;
            cuponRentaFija.NroDiasCupon = cupon.Dias;
            cuponRentaFija.TasaCupon = cupon.Tasa;
            cuponRentaFija.ImporteCupon = cupon.MontoCupon;
            cuponRentaFija.ImporteInteres = cupon.MontoInteres;
            cuponRentaFija.ImporteAmortizacion = cupon.MontoAmortizacion;
            cuponRentaFija.SaldoAmortizacion = cupon.SaldoPorAmortizar;
            cuponRentaFija.FactorCupon = cupon.Factor;
            cuponRentaFija.IndEstadoCupon = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.EstadoCupon, cupon.IdIndicadorEstadoCupon).Id;
            cuponRentaFija.LoginActualizacion = loginActualizacion;
            cuponRentaFija.FechaHoraActualizacion = DateTime.Now;
            cuponRentaFija.UsuarioActualizacion = Constants.UserSystem;

            SaveCuponRentaFija(cuponRentaFija);
        }

        void AddNewCertificadoDepositoCupon(InstrumentoCuponeraListadoDTO cupon, int idCertificadoDeposito, string loginActualizacion)
        {
            InstrumentoCertificadoDepositoCortoPlazoCupon cuponCertificadoDeposito = new InstrumentoCertificadoDepositoCortoPlazoCupon();
            cuponCertificadoDeposito.IdCertificadoDepositoCortoPlazo = idCertificadoDeposito;
            cuponCertificadoDeposito.NumeroCupon = cupon.NumeroCupon;
            cuponCertificadoDeposito.FechaInicio = Convert.ToDateTime(cupon.FechaInicio);
            cuponCertificadoDeposito.FechaCorte = Convert.ToDateTime(cupon.FechaCorte);
            cuponCertificadoDeposito.FechaPago = Convert.ToDateTime(cupon.FechaPago);
            cuponCertificadoDeposito.IndEstadoVigenciaCupon = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.EstadoVigenciaCupon, cupon.IdIndicador).Id;
            cuponCertificadoDeposito.NroDiasCupon = cupon.Dias;
            cuponCertificadoDeposito.TasaCupon = cupon.Tasa;
            cuponCertificadoDeposito.ImporteCupon = cupon.MontoCupon;
            cuponCertificadoDeposito.ImporteInteres = cupon.MontoInteres;
            cuponCertificadoDeposito.ImporteAmortizacion = cupon.MontoAmortizacion;
            cuponCertificadoDeposito.SaldoAmortizacion = cupon.SaldoPorAmortizar;
            cuponCertificadoDeposito.FactorCupon = cupon.Factor;
            cuponCertificadoDeposito.IndEstadoCupon = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive((int)eIndicador.EstadoCupon, cupon.IdIndicadorEstadoCupon).Id;
            cuponCertificadoDeposito.LoginActualizacion = loginActualizacion;
            cuponCertificadoDeposito.FechaHoraActualizacion = DateTime.Now;
            cuponCertificadoDeposito.UsuarioActualizacion = Constants.UserSystem;

            SaveCuponCertificadoDeposto(cuponCertificadoDeposito);
        }

        void SaveCuponRentaFija(InstrumentoRentaFijaCupon cupon)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(cupon))
            {
                iInstrumentoRentaFijaCuponRepository.Add(cupon);
                iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoRentaFijaCupon>(cupon));
        }

        void SaveCuponCertificadoDeposto(InstrumentoCertificadoDepositoCortoPlazoCupon cupon)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(cupon))
            {
                iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.Add(cupon);
                iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoCertificadoDepositoCortoPlazoCupon>(cupon));
        }

        void SaveInstrumento(Instrumento instrumento)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumento))
            {
                iInstrumentoRepository.Add(instrumento);
                iInstrumentoRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<Instrumento>(instrumento));

        }
        void SaveInstrumentoAccion(InstrumentoAccion instrumentoAccion)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoAccion))
            {
                iInstrumentoAccionRepository.Add(instrumentoAccion);
                iInstrumentoAccionRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoAccion>(instrumentoAccion));

        }
        void SaveInstrumentoFuturo(InstrumentoFuturo instrumentoFuturo)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoFuturo))
            {
                iInstrumentoFuturoRepository.Add(instrumentoFuturo);
                iInstrumentoFuturoRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoFuturo>(instrumentoFuturo));

        }
        void SaveInstrumentoOpcion(InstrumentoOpcion instrumentoOpcion)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoOpcion))
            {
                iInstrumentoOpcionRepository.Add(instrumentoOpcion);
                iInstrumentoOpcionRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoOpcion>(instrumentoOpcion));

        }
        void SaveInstrumentoCertificadoSuscripcionPreferente(InstrumentoCertificadoSuscripcionPreferente instrumentoCertificadoSuscripcionPreferente)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoCertificadoSuscripcionPreferente))
            {
                iInstrumentoCertificadoSuscripcionPreferenteRepository.Add(instrumentoCertificadoSuscripcionPreferente);
                iInstrumentoCertificadoSuscripcionPreferenteRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoCertificadoSuscripcionPreferente>(instrumentoCertificadoSuscripcionPreferente));

        }
        void SaveInstrumentoNotaEstructurada(InstrumentoNotaEstructurada instrumentoNotaEstructurada)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoNotaEstructurada))
            {
                iInstrumentoNotaEstructuradaRepository.Add(instrumentoNotaEstructurada);
                iInstrumentoNotaEstructuradaRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoNotaEstructurada>(instrumentoNotaEstructurada));

        }
        void SaveInstrumentoFondoMutuo(InstrumentoFondoMutuo instrumentoFondoMutuo)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoFondoMutuo))
            {
                iInstrumentoFondoMutuoRepository.Add(instrumentoFondoMutuo);
                iInstrumentoFondoMutuoRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoFondoMutuo>(instrumentoFondoMutuo));

        }
        void SaveInstrumentoRentaFija(InstrumentoRentaFija instrumentoRentaFija)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoRentaFija))
            {
                iInstrumentoRentaFijaRepository.Add(instrumentoRentaFija);
                iInstrumentoRentaFijaRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoRentaFija>(instrumentoRentaFija));

        }
        void SaveInstrumentoRentaFijaCupon(InstrumentoRentaFijaCupon instrumentoRentaFijaCupon)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoRentaFijaCupon))
            {
                iInstrumentoRentaFijaCuponRepository.Add(instrumentoRentaFijaCupon);
                iInstrumentoRentaFijaCuponRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoRentaFijaCupon>(instrumentoRentaFijaCupon));

        }
        void SaveInstrumentoFondoAlternativo(InstrumentoFondoAlternativo instrumentoFondoAlternativo)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoFondoAlternativo))
            {
                iInstrumentoFondoAlternativoRepository.Add(instrumentoFondoAlternativo);
                iInstrumentoFondoAlternativoRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoFondoAlternativo>(instrumentoFondoAlternativo));

        }
        void SaveInstrumentoCertificadoDepositoCortoPlazo(InstrumentoCertificadoDepositoCortoPlazo instrumentoCertificadoDepositoCortoPlazo)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoCertificadoDepositoCortoPlazo))
            {
                iInstrumentoCertificadoDepositoCortoPlazoRepository.Add(instrumentoCertificadoDepositoCortoPlazo);
                iInstrumentoCertificadoDepositoCortoPlazoRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoCertificadoDepositoCortoPlazo>(instrumentoCertificadoDepositoCortoPlazo));

        }
        void SaveInstrumentoCertificadoDepositoCortoPlazoCupon(InstrumentoCertificadoDepositoCortoPlazoCupon instrumentoCertificadoDepositoCortoPlazoCupon)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(instrumentoCertificadoDepositoCortoPlazoCupon))
            {
                iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.Add(instrumentoCertificadoDepositoCortoPlazoCupon);
                iInstrumentoCertificadoDepositoCortoPlazoCuponRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoCertificadoDepositoCortoPlazoCupon>(instrumentoCertificadoDepositoCortoPlazoCupon));

        }


        void SaveFondoAlternativoTasa(InstrumentoFondoAlternativoTasa fondoAlternativoTasa)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(fondoAlternativoTasa))
            {
                iFondoAlternativoTasaRepository.Add(fondoAlternativoTasa);
                iFondoAlternativoTasaRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoFondoAlternativoTasa>(fondoAlternativoTasa));

        }
        void SaveFondoAlternativoComprometido(InstrumentoFondoAlternativoComprometido fondoAlternativoComprometido)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(fondoAlternativoComprometido))
            {
                iFondoAlternativoComprometidoRepository.Add(fondoAlternativoComprometido);
                iFondoAlternativoComprometidoRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoFondoAlternativoComprometido>(fondoAlternativoComprometido));

        }
        void SaveFondoAlternativoLlamada(InstrumentoFondoAlternativoLlamada fondoAlternativoLlamada)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(fondoAlternativoLlamada))
            {
                iFondoAlternativoLlamadaRepository.Add(fondoAlternativoLlamada);
                iFondoAlternativoLlamadaRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoFondoAlternativoLlamada>(fondoAlternativoLlamada));

        }
        void SaveFondoAlternativoDetalleComprometido(InstrumentoFondoAlternativoDetalleComprometido fondoAlternativoDetalleComprometido)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(fondoAlternativoDetalleComprometido))
            {
                iFondoAlternativoDetalleComprometidoRepository.Add(fondoAlternativoDetalleComprometido);
                iFondoAlternativoDetalleComprometidoRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoFondoAlternativoDetalleComprometido>(fondoAlternativoDetalleComprometido));

        }
        void SaveFondoAlternativoDetalleLlamada(InstrumentoFondoAlternativoDetalleLlamada fondoAlternativoDetalleLlamada)
        {
            var validator = EntityValidatorFactory.CreateValidator();
            if (validator.IsValid(fondoAlternativoDetalleLlamada))
            {
                iFondoAlternativoDetalleLlamadaRepository.Add(fondoAlternativoDetalleLlamada);
                iFondoAlternativoDetalleLlamadaRepository.UnitOfWork.Commit();
            }
            else
                throw new ApplicationValidationErrorsException(validator.GetInvalidMessages<InstrumentoFondoAlternativoDetalleLlamada>(fondoAlternativoDetalleLlamada));

        }
        void VerifyNombreInstrumentoIsUnique(string codigoSbs, int idInstrumento)
        {
            if (codigoSbs.Length != (int)CustomLength.CodeSBSInstrumento)
                throw new Exception(String.Format(mensajeGenericoES.MSJ_002_Codigo_Sbs_Instrumento_Minimo_Caracteres, (int)CustomLength.CodeSBSInstrumento));

            int existingsInstrumento = iInstrumentoRepository.GetFiltered(x => (x.CodigoSbs.ToLower().Equals(codigoSbs.ToLower()) && x.IdInstrumento != idInstrumento)).Count();
            string accionMensaje = (idInstrumento <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;

            if (existingsInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_SBS_InstrumentoUnico));
        }
        void VerifyInstrumentoFuturoIsUnique(InstrumentoFuturoDTO instrumentoFuturoDTO)
        {
            int existingsTickerInstrumento = iInstrumentoFuturoRepository.GetFiltered(x => (x.Ticker.ToLower().Equals(instrumentoFuturoDTO.Ticker.ToLower()) && x.IdFuturo != instrumentoFuturoDTO.IdFuturo)).Count();
            string accionMensaje = (instrumentoFuturoDTO.IdFuturo <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsTickerInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Ticker_InstrumentoUnico));
        }
        void VerifyInstrumentoOpcionIsUnique(InstrumentoOpcionDTO instrumentoOpcionDTO)
        {
            int existingsTickerInstrumento = iInstrumentoOpcionRepository.GetFiltered(x => (x.Ticker.ToLower().Equals(instrumentoOpcionDTO.Ticker.ToLower()) && x.IdOpcion != instrumentoOpcionDTO.IdOpcion)).Count();
            string accionMensaje = (instrumentoOpcionDTO.IdOpcion <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsTickerInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Ticker_InstrumentoUnico));
        }
        void VerifyInstrumentoCertificadoSuscripcionPreferenteIsUnique(InstrumentoCertificadoSuscripcionPreferenteDTO instrumentoSuscripcionPreferenteDTO)
        {
            int existingsIsinInstrumento = iInstrumentoCertificadoSuscripcionPreferenteRepository.GetFiltered(x => (x.CodigoIsin.ToLower().Equals(instrumentoSuscripcionPreferenteDTO.CodigoIsin.ToLower()) && x.IdCertificadoSuscripcionPreferente != instrumentoSuscripcionPreferenteDTO.IdCertificadoSuscripcionPreferente)).Count();
            int existingsNemotecnicoInstrumento = iInstrumentoCertificadoSuscripcionPreferenteRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoSuscripcionPreferenteDTO.Nemotecnico.ToLower()) && x.IdCertificadoSuscripcionPreferente != instrumentoSuscripcionPreferenteDTO.IdCertificadoSuscripcionPreferente)).Count();

            string accionMensaje = (instrumentoSuscripcionPreferenteDTO.IdCertificadoSuscripcionPreferente <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsIsinInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));

            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));

        }
        void VerifyInstrumentoFondoMutuoIsUnique(InstrumentoFondoMutuoDTO instrumentoFondoMutuoDTO)
        {
            string accionMensaje = (instrumentoFondoMutuoDTO.IdFondoMutuo <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;

            int existingsNemotecnicoInstrumento = iInstrumentoFondoMutuoRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoFondoMutuoDTO.Nemotecnico.ToLower()) && x.IdFondoMutuo != instrumentoFondoMutuoDTO.IdFondoMutuo)).Count();
            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));

            if (!string.IsNullOrWhiteSpace(instrumentoFondoMutuoDTO.CodigoIsin))
            {
                int existingsIsinInstrumento = iInstrumentoFondoMutuoRepository.GetFiltered(x => (x.CodigoIsin.ToLower().Equals(instrumentoFondoMutuoDTO.CodigoIsin.ToLower()) && x.IdFondoMutuo != instrumentoFondoMutuoDTO.IdFondoMutuo)).Count();
                if (existingsIsinInstrumento > 0)
                    throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));
            }
            if (!string.IsNullOrWhiteSpace(instrumentoFondoMutuoDTO.CodigoCusip))
            {
                int existingsCodigoCusipgInstrumento = iInstrumentoFondoMutuoRepository.GetFiltered(x => (x.CodigoCusip.ToLower().Equals(instrumentoFondoMutuoDTO.CodigoCusip.ToLower()) && x.IdFondoMutuo != instrumentoFondoMutuoDTO.IdFondoMutuo)).Count();
                if (existingsCodigoCusipgInstrumento > 0)
                    throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Cusip_InstrumentoUnico));

            }
            if (!string.IsNullOrWhiteSpace(instrumentoFondoMutuoDTO.CodigoBloomberg))
            {
                int existingsCodigoBloombergInstrumento = iInstrumentoFondoMutuoRepository.GetFiltered(x => (x.CodigoBloomberg.ToLower().Equals(instrumentoFondoMutuoDTO.CodigoBloomberg.ToLower()) && x.IdFondoMutuo != instrumentoFondoMutuoDTO.IdFondoMutuo)).Count();
                if (existingsCodigoBloombergInstrumento > 0)
                    throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Bloomberg_InstrumentoUnico));
            }
            if (!string.IsNullOrWhiteSpace(instrumentoFondoMutuoDTO.NombreFondo))
            {
                int existingsnombreFondoInstrumento = iInstrumentoFondoMutuoRepository.GetFiltered(x => (x.NombreFondo.ToLower().Equals(instrumentoFondoMutuoDTO.NombreFondo.ToLower()) && x.IdFondoMutuo != instrumentoFondoMutuoDTO.IdFondoMutuo)).Count();
                if (existingsnombreFondoInstrumento > 0)
                    throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Nombre_Fondo_InstrumentoUnico));
            }



        }
        void VerifyInstrumentoNotaEstructuradaIsUnique(InstrumentoNotaEstructuradaDTO instrumentoNotaEstructuradaDTO)
        {
            int existingsIsinInstrumento = 0;
            if (!string.IsNullOrEmpty(instrumentoNotaEstructuradaDTO.CodigoIsin))
                existingsIsinInstrumento = iInstrumentoNotaEstructuradaRepository.GetFiltered(x => (x.CodigoIsin.ToLower().Equals(instrumentoNotaEstructuradaDTO.CodigoIsin.ToLower()) && x.IdNotaEstructurada != instrumentoNotaEstructuradaDTO.IdNotaEstructurada)).Count();
            int existingsNemotecnicoInstrumento = iInstrumentoNotaEstructuradaRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoNotaEstructuradaDTO.Nemotecnico.ToLower()) && x.IdNotaEstructurada != instrumentoNotaEstructuradaDTO.IdNotaEstructurada)).Count();
            string accionMensaje = (instrumentoNotaEstructuradaDTO.IdNotaEstructurada <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsIsinInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));

            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));
        }
        void VerifyInstrumentoAccionIsUnique(InstrumentoAccionDTO instrumentoAccionDTO)
        {
            int existingsIsinInstrumento = iInstrumentoAccionRepository.GetFiltered(x => (x.CodIsin.ToLower().Equals(instrumentoAccionDTO.CodIsin.ToLower()) && x.IdAccion != instrumentoAccionDTO.IdAccion)).Count();
            int existingsNemotecnicoInstrumento = iInstrumentoAccionRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoAccionDTO.Nemotecnico.ToLower()) && x.IdAccion != instrumentoAccionDTO.IdAccion)).Count();
            string accionMensaje = (instrumentoAccionDTO.IdAccion <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsIsinInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));

            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));
        }
        void VerifyInstrumentoAccionAdrAdsIsUnique(InstrumentoAccionAdrAdsDTO instrumentoAccionAdrAdsDTO, int idAccion)
        {
            int existingsIsinInstrumento = iInstrumentoAccionRepository.GetFiltered(x => (x.CodIsin.ToLower().Equals(instrumentoAccionAdrAdsDTO.CodIsin.ToLower()) && x.IdAccion != idAccion)).Count();
            int existingsNemotecnicoInstrumento = iInstrumentoAccionRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoAccionAdrAdsDTO.Nemotecnico.ToLower()) && x.IdAccion != idAccion)).Count();
            string accionMensaje = (idAccion <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsIsinInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));

            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));
        }
        void VerifyInstrumentoRentaFijaGdnIsUnique(InstrumentoRentaFijaGdnDTO instrumentoRentaFijaGdnDTO, int idRentaFija)
        {
            int existingsIsinInstrumento = iInstrumentoRentaFijaRepository.GetFiltered(x => (x.CodigoIsin.ToLower().Equals(instrumentoRentaFijaGdnDTO.CodIsin.ToLower()) && x.IdRentaFija != idRentaFija)).Count();
            int existingsNemotecnicoInstrumento = iInstrumentoRentaFijaRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoRentaFijaGdnDTO.Nemotecnico.ToLower()) && x.IdRentaFija != idRentaFija)).Count();
            string accionMensaje = (idRentaFija <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsIsinInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));

            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));
        }
        void VerifyInstrumentoRentaFijaIsUnique(InstrumentoRentaFijaDTO instrumentoRentaFijaDTO)
        {
            int existingsIsinInstrumento = iInstrumentoRentaFijaRepository.GetFiltered(x => (x.CodigoIsin.ToLower().Equals(instrumentoRentaFijaDTO.CodigoIsin.ToLower()) && x.IdRentaFija != instrumentoRentaFijaDTO.IdRentaFija)).Count();
            int existingsNemotecnicoInstrumento = iInstrumentoRentaFijaRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoRentaFijaDTO.Nemotecnico.ToLower()) && x.IdRentaFija != instrumentoRentaFijaDTO.IdRentaFija)).Count();
            string accionMensaje = (instrumentoRentaFijaDTO.IdRentaFija <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsIsinInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));

            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));
        }
        void VerifyInstrumentoCertificadoDepositoIsUnique(InstrumentoCertificadoDepositoCortoPlazoDTO instrumentoCertificadoDepositoDTO)
        {
            int existingsIsinInstrumento = iInstrumentoCertificadoDepositoCortoPlazoRepository.GetFiltered(x => (x.CodigoIsin.ToLower().Equals(instrumentoCertificadoDepositoDTO.CodigoIsin.ToLower()) && x.IdCertificadoDepositoCortoPlazo != instrumentoCertificadoDepositoDTO.IdCertificadoDepositoCortoPlazo)).Count();
            int existingsCavaliInstrumento = iInstrumentoCertificadoDepositoCortoPlazoRepository.GetFiltered(x => (!string.IsNullOrEmpty(x.CodigoCavali) && x.CodigoCavali.ToLower().Equals(instrumentoCertificadoDepositoDTO.CodigoCavali.ToLower()) && x.IdCertificadoDepositoCortoPlazo != instrumentoCertificadoDepositoDTO.IdCertificadoDepositoCortoPlazo)).Count();
            int existingsNemotecnicoInstrumento = iInstrumentoCertificadoDepositoCortoPlazoRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoCertificadoDepositoDTO.Nemotecnico.ToLower()) && x.IdCertificadoDepositoCortoPlazo != instrumentoCertificadoDepositoDTO.IdCertificadoDepositoCortoPlazo)).Count();
            string accionMensaje = (instrumentoCertificadoDepositoDTO.IdCertificadoDepositoCortoPlazo <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsIsinInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));

            if (existingsCavaliInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Cavali_InstrumentoUnico));

            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));
        }
        public MonedaDTO VerifyMonedaIndexada(int indIndicadorInteres, int idMoneda, bool esBono)
        {
            MonedaDTO monedaIndexada = null;
            IndicadorDTO indexadoInflacion = iIndicadorAppService.GetByTipoIndicadorAndIndicadorAndActive(esBono ? (int)eIndicador.IndicadorInteresesBonos : (int)eIndicador.IndicadorInteresesCertificadoDeposito, indIndicadorInteres);
            if (indexadoInflacion.IdIndicador.Equals((int)eIndicadorInteresesBonos.IndexadoInflacion))
            {
                monedaIndexada = iMonedaAppService.GetMonedaIndexada(idMoneda);
                if (monedaIndexada == null)
                    throw new ArgumentException("La moneda no cuenta con una Moneda Indexada vigente y habilitada para el cálculo de cupones.");
                return monedaIndexada;
            }
            else
                return new MonedaDTO();
        }
        void VerifyInstrumentoFondoAlternativoIsUnique(InstrumentoFondoAlternativoDTO instrumentoFondoAlternativoDTO)
        {
            int existingsIsinInstrumento = iInstrumentoFondoAlternativoRepository.GetFiltered(x => (x.CodigoIsin.ToLower().Equals(instrumentoFondoAlternativoDTO.CodigoIsin.ToLower()) && x.IdFondoAlternativo != instrumentoFondoAlternativoDTO.IdFondoAlternativo)).Count();
            int existingsNombreFondoInstrumento = iInstrumentoFondoAlternativoRepository.GetFiltered(x => (x.NombreFondo.Trim().ToLower().Equals(instrumentoFondoAlternativoDTO.NombreFondo.Trim().ToLower()) && x.IdFondoAlternativo != instrumentoFondoAlternativoDTO.IdFondoAlternativo)).Count();
            int existingsNemotecnicoInstrumento = iInstrumentoFondoAlternativoRepository.GetFiltered(x => (x.Nemotecnico.ToLower().Equals(instrumentoFondoAlternativoDTO.Nemotecnico.ToLower()) && x.IdFondoAlternativo != instrumentoFondoAlternativoDTO.IdFondoAlternativo)).Count();
            string accionMensaje = (instrumentoFondoAlternativoDTO.IdFondoAlternativo <= 0) ? "{0}" : mensajeGenericoES.error_ActualizarElemento;
            if (existingsIsinInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Isin_InstrumentoUnico));

            if (existingsNombreFondoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_001_Nombre_Fondo_Unico));

            if (existingsNemotecnicoInstrumento > 0)
                throw new DuplicateNameException(string.Format(accionMensaje, mensajeGenericoES.MSJ_002_Codigo_Nemotecnico_InstrumentoUnico));
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// <see cref="M:System.IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            //dispose all resources
            iInstrumentoRepository.Dispose();
        }





        #endregion


        public InstrumentoListadoDTO[] GetAllInstrumentoByStock(int IdFechaInicio, int IdFechaFin)
        {
            return iInstrumentoDataRepository.GetAllInstrumentoByStock(IdFechaInicio, IdFechaFin);
        }

        public BusquedaInstrumentoDataDto[] GetInstrumentosForDividendos(BusquedaInstrumentoFilterDto filtros)
        {
            return iInstrumentoDataRepository.GetInstrumentosForDividendos(filtros);
        }

        public Boolean ValidaCambioPrecioInstrumentoCarga(int IdLote)
        {
            return iInstrumentoDataRepository.ValidaCambioPrecioInstrumentoCarga(IdLote);
        }

        public FAActualizacionPrecioReversaDTO[] GetActualizacionPrecioOrdenesInversion(int IdActualizacionPrecio)
        {
            return iInstrumentoDataRepository.GetActualizacionPrecioOrdenesInversion(IdActualizacionPrecio);
        }

        public void LimpiarActualizacionPrecio(int IdActualizacionPrecio)
        {
            iInstrumentoDataRepository.LimpiarActualizacionPrecio(IdActualizacionPrecio);
        }

        public InstrumentoPrecioDTO GetUltimoInstrumentoPrecio(int idInstrumento)
        {
            return iInstrumentoDataRepository.GetUltimoInstrumentoPrecio(idInstrumento);
        }
    }
}