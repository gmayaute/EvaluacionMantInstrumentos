module app.mesadineroapp.instrumentoAccion {
    'use strict';
    export interface IinstrumentoAccionDetailScope {
        currentInstrumentoAccion: app.common.IInstrumentoAccion;
        isEditing: boolean;
        isRemovingOrAnnulling: boolean;
        isAnnulling: boolean;
        tittle: string;
        message: string;
        ctrlEstados: wijmo.input.ComboBox;
        isCurrenItemDisabled: boolean;
        operationText: string;
        comentarioAnulacionVisibility: boolean;
        requiredFieldsVisibility: boolean;
        isDeleting: boolean;
        currentItem: number;
    }

    export class InstrumentoAccionDetailController implements IinstrumentoAccionDetailScope {
        public static controllerId: string = 'app.mesadineroapp.instrumentoAccion.instrumentoAccionDetailController';

        isCurrenItemDisabled: boolean;
        requiredFieldsVisibility: boolean;
        comentarioAnulacionVisibility: boolean;
        isCreate: boolean;
        isUpdating: boolean;
        isEditing: boolean;
        isDeleting: boolean;
        isAnnulling: boolean;
        isRemovingOrAnnulling: boolean;
        tittle: string;
        message: string;
        operationText: string;
        currentItemForm: ng.IFormController;
        currentItem: number;
        currentInstrumentoAccion: app.common.IInstrumentoAccion;

        tiposAcciones: app.common.ITipoInstrumento[] = [];
        emisores: app.common.IEntidadRol[] = [];
        emisoresAll: app.common.IEntidadRol[] = [];
        monedas: app.common.IMoneda[] = [];
        monedasAll: app.common.IMoneda[] = [];
        listaDualMoneda: app.common.IMoneda[] = [];
        estados: app.common.IIndicador[] = [];
        habilitaciones: app.common.IIndicador[] = [];
        tiposCustodia: app.common.IIndicador[] = [];
        unidades: app.common.IIndicador[] = [];
        clasificacionesRiesgoSBS: app.common.IClasificacionRiesgo[] = [];
        focosGeografico: app.common.IIndicador[] = [];
        clases: app.common.IIndicador[] = [];
        regiones: app.common.IIndicador[] = [];
        categorias: app.common.IIndicador[] = [];
        paises: app.common.IIndicador[] = [];
        familias: app.common.IIndicador[] = [];

        ctrlEstados: wijmo.input.ComboBox;
        ctrlTiposAcciones: wijmo.input.ComboBox;
        ctrlEmisores: wijmo.input.ComboBox;
        ctrlMonedas: wijmo.input.ComboBox;
        ctrlDualMoneda: wijmo.input.ComboBox;
        ctrlUnidades: wijmo.input.ComboBox;
        ctrlHabilitaciones: wijmo.input.ComboBox;
        ctrlClasificacionRiesgoSBS: wijmo.input.ComboBox;
        ctrlFocosGeografico: wijmo.input.ComboBox;
        ctrlClases: wijmo.input.ComboBox;
        ctrlRegiones: wijmo.input.ComboBox;
        ctrlCategorias: wijmo.input.ComboBox;
        ctrlPaises: wijmo.input.ComboBox;
        ctrlFamilia: wijmo.input.ComboBox;

        codigoSbsIngresado: string = "";
        idDualMoneda: number = 0;

        isCurrenItemDisabledInelegible: boolean;
        isCurrenItemDisabledHabilitado: boolean;
        classMercadoPopUp: string;
        fechaInitial: boolean = false;

        static $inject = [
            'instrumentoAccion',
            'operationType',
            'doubleClickDisabled',
            'totalItems',
            '$uibModalInstance',
            'app.blocks.ModalService',
            'toastr',
            '$scope',
            '$compile',
            '$window',
            'appSetting',
            'sharedLabels',
            'currentUser',
            'customLength',
            '$q',
            'instrumentoAccionLabels',
            'app.services.SharedIndicadorService',
            'app.services.InstrumentoAccionService',
            'app.services.TipoInstrumentoService',
            'app.services.EntidadService',
            'app.services.MonedaService',
            'app.services.ClasificacionRiesgoService'
        ];
        constructor(
            private initialInstrumentoAccion: app.common.IInstrumentoAccion,
            private operationType: number,
            private doubleClickDisabled: boolean,
            private totalItems: number,
            private $uibModalInstance: any,
            private modalService: app.blocks.IModalService,
            private claimMessage: toastr.IToastrService,
            private $scope: ng.IScope,
            private $compile: ng.ICompileService,
            private $window: ng.IWindowService,
            private appSetting: IAppSetting,
            private sharedLabels: SharedLabels,
            private currentUser: ICurrentUser,
            private customLength: CustomLength,
            private $q: ng.IQService,
            private instrumentoAccionLabels: app.InstrumentoAccionLabels,
            private sharedIndicadorService: app.services.ISharedIndicadorService,
            private instrumentoAccionService: app.services.IInstrumentoAccionService,
            private tipoInstrumentoService: app.services.ITipoInstrumentoService,
            private entidadService: app.services.IEntidadService,
            private monedaService: app.services.IMonedaService,
            private clasificacionRiesgoService: app.services.IClasificacionRiesgoService
        ) {
            this.$compile = $compile
            this.$scope = $scope;
            this.$window.onhashchange = function () { $uibModalInstance.close('cancel'); }
            this.initialize();
        }

        initialize() {
            try {
                var vm = this;
                vm.isEditing = (vm.operationType > app.common.eOperation.AddNew) ? true : false;//When the operation is  Update ||Remove || Annul
                vm.tittle = (vm.isEditing || (vm.operationType == -1)) ? "" : vm.sharedLabels.newButtonText;
                vm.isCreate = (vm.operationType == app.common.eOperation.AddNew) ? true : false;
                vm.isUpdating = vm.operationType == app.common.eOperation.Update ? true : false;
                vm.isDeleting = (vm.operationType == app.common.eOperation.Remove) ? true : false;
                vm.requiredFieldsVisibility = (vm.operationType == app.common.eOperation.Remove) ? false : ((vm.operationType == app.common.eOperation.AddNew || vm.operationType == app.common.eOperation.Annul) ? true : ((vm.isCurrenItemDisabled) ? false : true));
                vm.message = (vm.isEditing || (vm.operationType == -1)) ? vm.sharedLabels.updatedByMessage : "";
                vm.isRemovingOrAnnulling = (vm.operationType == app.common.eOperation.Remove || vm.operationType == app.common.eOperation.Annul) ? true : false;
                vm.isAnnulling = (vm.operationType == app.common.eOperation.Annul) ? true : false;
                vm.operationText = (vm.operationType == app.common.eOperation.Remove) ? vm.sharedLabels.eliminacionMessagePopup : vm.sharedLabels.anulacionMessagePopup;
                vm.comentarioAnulacionVisibility = false;
                vm.currentInstrumentoAccion = vm.initialInstrumentoAccion;

                vm.preload();

                vm.$scope.$watch(() => {
                    $(".wj-header").css("background-color", vm.appSetting.codigoColor);
                    $(".modal-header").css("background-color", vm.appSetting.codigoColor);
                    $(".btn_color_selecc").css("background-color", vm.appSetting.codigoColor);
                    return vm
                }, (valorNuevo, valorAntiguo) => { });
            }
            catch (e) { alert(e); }
        }

        preload(): void {
            var vmd = this;

            var tiposAccionesPromise = vmd.tipoInstrumentoService.GetAllByGrupoInstrumento(app.common.eGrupoInstrumento.Acciones, vmd.sharedLabels.selectText);
            var emisoresPromise = vmd.entidadService.getActiveAllByRol(app.common.eEntidad.Emisor, vmd.sharedLabels.selectText);
            var monedasPromise = vmd.monedaService.getAllActiveHabilitado(vmd.sharedLabels.selectText); //vmd.monedaService.getAll(vmd.sharedLabels.selectText);
            var estadosPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.Estado, vmd.sharedLabels.selectText);
            var habilitacionesPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.TipoHabilitacion, vmd.sharedLabels.selectText);
            var tiposCustodiaPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.TipoCustodia, vmd.sharedLabels.selectText);
            var unidadesPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.Unidades, vmd.sharedLabels.selectText);
            var clasificacionesRiesgoSBSPromise = vmd.clasificacionRiesgoService.getAllClasificionriesgoActiveByIdExcluir(0, vmd.sharedLabels.selectText);
            var focosGeograficoPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.FocoGeografico, vmd.sharedLabels.selectText);
            var clasesPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.Clase, vmd.sharedLabels.selectText);
            var regionesPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.Region, vmd.sharedLabels.selectText);
            var categoriasPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.Categoria, vmd.sharedLabels.selectText);
            var paisesPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.Pais, vmd.sharedLabels.selectText);
            var familiasPromise = vmd.sharedIndicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.Familia, vmd.sharedLabels.selectText);
            var monedasAllPromise = vmd.monedaService.getAllMonedasActiveByIdMonedaExcluir(0, vmd.sharedLabels.selectText); //vmd.monedaService.getAll(vmd.sharedLabels.selectText);
            var emisoresAllPromise = vmd.entidadService.getAllBase();
            var tiposAdrPromise = vmd.tipoInstrumentoService.getAllByIdIndicadorSubTipo(app.common.eSubTipoSbs.ADR);
            var tiposAdsPromise = vmd.tipoInstrumentoService.getAllByIdIndicadorSubTipo(app.common.eSubTipoSbs.ADS);

            vmd.appSetting.isLoading = true;
            vmd.$q.all([tiposAccionesPromise, emisoresPromise, monedasPromise, estadosPromise, habilitacionesPromise, tiposCustodiaPromise, unidadesPromise, clasificacionesRiesgoSBSPromise,
                focosGeograficoPromise, clasesPromise, regionesPromise, categoriasPromise, paisesPromise, familiasPromise, emisoresAllPromise, monedasAllPromise,
                tiposAdrPromise, tiposAdsPromise
            ])
                .then((results) => {
                    vmd.tiposAcciones = (<app.common.ITipoInstrumento[]>results[0]);

                    if (vmd.initialInstrumentoAccion.idInstrumento > 0) {
                    } else {
                        var tiposAdr = (<app.common.ITipoInstrumento[]>results[16]);
                        var tiposAds = (<app.common.ITipoInstrumento[]>results[17]);

                        if (tiposAdr != null)
                            tiposAdr.forEach((value, index) => {
                                vmd.tiposAcciones = vmd.tiposAcciones.filter(x => x.idTipoInstrumento != value.idTipoInstrumento);
                            })
                        if (tiposAds != null)
                            tiposAds.forEach((value, index) => {
                                vmd.tiposAcciones = vmd.tiposAcciones.filter(x => x.idTipoInstrumento != value.idTipoInstrumento);
                            })
                    }

                    vmd.emisores = (<app.common.IEntidadRol[]>results[1]);
                    vmd.monedas = (<app.common.IMoneda[]>results[2]);
                    vmd.estados = (<app.common.IIndicador[]>results[3]);
                    vmd.habilitaciones = (<app.common.IIndicador[]>results[4]);
                    vmd.tiposCustodia = (<app.common.IIndicador[]>results[5]);
                    vmd.unidades = (<app.common.IIndicador[]>results[6]);
                    vmd.clasificacionesRiesgoSBS = (<app.common.IClasificacionRiesgo[]>results[7]);
                    vmd.focosGeografico = (<app.common.IIndicador[]>results[8]);
                    vmd.clases = (<app.common.IIndicador[]>results[9]);
                    vmd.regiones = (<app.common.IIndicador[]>results[10]);
                    vmd.categorias = (<app.common.IIndicador[]>results[11]);
                    vmd.paises = (<app.common.IIndicador[]>results[12]);
                    vmd.familias = (<app.common.IIndicador[]>results[13]);
                    vmd.emisoresAll = (<app.common.IEntidadRol[]>results[14]);
                    vmd.monedasAll = (<app.common.IMoneda[]>results[15]);
                    vmd.ctrlTiposAcciones.itemsSource = vmd.tiposAcciones;
                    vmd.ctrlEmisores.itemsSource = vmd.emisores;
                    vmd.ctrlMonedas.itemsSource = vmd.monedas;
                    vmd.ctrlDualMoneda.itemsSource = vmd.monedas;
                    vmd.ctrlUnidades.itemsSource = vmd.unidades;
                    vmd.ctrlHabilitaciones.itemsSource = vmd.habilitaciones;
                    vmd.ctrlClasificacionRiesgoSBS.itemsSource = vmd.clasificacionesRiesgoSBS;
                    vmd.ctrlFocosGeografico.itemsSource = vmd.focosGeografico;
                    vmd.ctrlClases.itemsSource = vmd.clases;
                    vmd.ctrlRegiones.itemsSource = vmd.regiones;
                    vmd.ctrlCategorias.itemsSource = vmd.categorias;
                    vmd.ctrlPaises.itemsSource = vmd.paises;
                    vmd.ctrlFamilia.itemsSource = vmd.familias;
                    if (vmd.ctrlEstados)
                        vmd.ctrlEstados.itemsSource = vmd.estados;
                    //this.appSetting.isLoading = false;
                    vmd.loadInstrumentoAccion();
                })
                .catch((error) => {
                    vmd.claimMessage.error(error.data);
                    vmd.appSetting.isLoading = false;
                })
        }

        loadInstrumentoAccion(): void {
            var vm = this;
            vm.appSetting.isLoading = true;
            if (vm.initialInstrumentoAccion.idInstrumento > 0) {
                vm.emisores = angular.copy(vm.emisoresAll);
                vm.instrumentoAccionService.getByIdInstrumentoAccion(vm.initialInstrumentoAccion.idInstrumento,
                    vm.initialInstrumentoAccion.idAccion).then(data => {
                        vm.currentInstrumentoAccion = data;
                        vm.setInstrumentoAccion();
                        vm.fechaInitial = true;
                        vm.appSetting.isLoading = false;
                    },
                    (error: common.IHttpPromiseCallbackErrorArg) => { vm.claimMessage.error(error.data); vm.appSetting.isLoading = true; });
            } else {
                vm.fechaInitial = true;
                vm.appSetting.isLoading = false;
                vm.classMercadoPopUp = 'col-xs-10';
            }
        }

        setInstrumentoAccion(): void {
            var vmd = this;
            //obtenemos los valores de fechas a partir del campo texto            
            vmd.currentInstrumentoAccion.fechaEmision = app.common.stringToDate(vmd.currentInstrumentoAccion.secuencialFechaEmision);
            vmd.currentInstrumentoAccion.fechaMontoColocado = app.common.stringToDate(vmd.currentInstrumentoAccion.secuencialFechaMontoColocado);
            vmd.currentInstrumentoAccion.fechaMontoEmitido = app.common.stringToDate(vmd.currentInstrumentoAccion.secuencialFechaMontoEmitido);
            vmd.currentInstrumentoAccion.fechaVencimiento = app.common.stringToDate(vmd.currentInstrumentoAccion.secuencialFechaVencimiento);

            //asignamos el select a los combos
            vmd.ctrlTiposAcciones.selectedValue = vmd.currentInstrumentoAccion.idTipoInstrumento;
            vmd.ctrlEmisores.selectedValue = vmd.currentInstrumentoAccion.idEmisor;
            vmd.ctrlMonedas.selectedValue = vmd.currentInstrumentoAccion.idMoneda;
            vmd.ctrlDualMoneda.selectedValue = vmd.currentInstrumentoAccion.idMonedaDual;
            vmd.ctrlUnidades.selectedValue = vmd.currentInstrumentoAccion.indTipoUnidadEmision;
            vmd.ctrlEstados.selectedValue = vmd.currentInstrumentoAccion.indActividad;
            vmd.ctrlHabilitaciones.selectedValue = vmd.currentInstrumentoAccion.indHabilitacionRiesgo;
            vmd.ctrlClasificacionRiesgoSBS.selectedValue = vmd.currentInstrumentoAccion.idClasificacionRiesgo;
            vmd.ctrlFocosGeografico.selectedValue = vmd.currentInstrumentoAccion.indFocoGeograficoEmision;
            vmd.ctrlClases.selectedValue = vmd.currentInstrumentoAccion.indClase;
            vmd.ctrlRegiones.selectedValue = vmd.currentInstrumentoAccion.indRegionEmision;
            vmd.ctrlCategorias.selectedValue = vmd.currentInstrumentoAccion.indCategoria;
            vmd.ctrlPaises.selectedValue = vmd.currentInstrumentoAccion.indPaisEmisor;
            vmd.ctrlFamilia.selectedValue = vmd.currentInstrumentoAccion.indFamilia;
            vmd.idDualMoneda = vmd.currentInstrumentoAccion.idMonedaDual;
            //cargamos la descripción calculada
            vmd.changeDescripcion();
            //Separamos los campos de código SBS
            vmd.currentInstrumentoAccion.codigoSbsJoined = vmd.currentInstrumentoAccion.codigoSbs;
            vmd.isCurrenItemDisabled = vmd.verifyCurrentItemIsDisabled();
            vmd.comentarioAnulacionVisibility = vmd.isCurrenItemDisabled ? true : (vmd.operationType == app.common.eOperation.Update || vmd.operationType == app.common.eOperation.Remove || vmd.operationType == -1) ? false : true;
            vmd.requiredFieldsVisibility = (vmd.operationType == app.common.eOperation.Remove) ? false : ((vmd.operationType == app.common.eOperation.AddNew || vmd.operationType == app.common.eOperation.Annul) ? true : ((vmd.isCurrenItemDisabled) ? false : true));
            vmd.currentItem = angular.copy(vmd.initialInstrumentoAccion.posicion);
            if (vmd.operationType == -1) {
                vmd.isCurrenItemDisabledInelegible = vmd.verifyCurrentItemIsDisabledInelegible();
                vmd.isCurrenItemDisabledHabilitado = vmd.verifyCurrentItemIsDisabledHabilitado();

                vmd.classMercadoPopUp = (vmd.isCurrenItemDisabledHabilitado == true && vmd.isCurrenItemDisabledInelegible == true) ? 'col-xs-7' : (vmd.isCurrenItemDisabledHabilitado == false && vmd.isCurrenItemDisabledInelegible == true) ? 'col-xs-9'
                    : (vmd.isCurrenItemDisabledHabilitado == false && vmd.isCurrenItemDisabledInelegible == false) ? 'col-xs-11' : (vmd.isCurrenItemDisabledHabilitado == true && vmd.isCurrenItemDisabledInelegible == false) ? 'col-xs-9' : 'col-xs-7';
            }
            else {
                vmd.classMercadoPopUp = 'col-xs-7';
            }
        }

        verifyCurrentItemIsDisabledInelegible(): boolean {
            var vmd = this;
            if (vmd.currentInstrumentoAccion != undefined) {
                return   vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(x => x.idIndicador == app.common.eEstado.Anulado)[0].id ? false :
                    (vmd.currentInstrumentoAccion.indHabilitacionRiesgo == vmd.habilitaciones.filter(x => x.idIndicador == app.common.eTipoHabilitacion.Habilitado)[0].id &&
                     vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(x => x.idIndicador == app.common.eEstado.Vigente)[0].id) ? false :
                    (vmd.currentInstrumentoAccion.indHabilitacionRiesgo == vmd.habilitaciones.filter(x => x.idIndicador == app.common.eTipoHabilitacion.InhabilitadoInelegible)[0].id &&
                     vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(x => x.idIndicador == app.common.eEstado.Vigente)[0].id) ? true :
                    (vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(x => x.idIndicador == app.common.eEstado.Vigente)[0].id) ? true : false;
            }
            return false;
        }
        verifyCurrentItemIsDisabledHabilitado(): boolean {
            var vmd = this;
            if (vmd.currentInstrumentoAccion != undefined) {
                return vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(x => x.idIndicador == app.common.eEstado.Anulado)[0].id ? false :
                      (vmd.currentInstrumentoAccion.indHabilitacionRiesgo == vmd.habilitaciones.filter(x => x.idIndicador == app.common.eTipoHabilitacion.Habilitado)[0].id &&
                       vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(x => x.idIndicador == app.common.eEstado.Vigente)[0].id) ? true :
                      (vmd.currentInstrumentoAccion.indHabilitacionRiesgo == vmd.habilitaciones.filter(x => x.idIndicador == app.common.eTipoHabilitacion.InhabilitadoInelegible)[0].id &&
                       vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(x => x.idIndicador == app.common.eEstado.Vigente)[0].id) ? true : false;
            }
            return false;
        }

        private cambiarEstado(id: number): void {
            var vmd = this;
            var isValid = vmd.validateRequiredFields();
            if (!isValid)
                return;
            switch (id) {
                case app.common.eTipoHabilitacion.Habilitado:
                    var modalOptions = {
                        cancelButtonText: vmd.sharedLabels.cancelButtonText,
                        actionButtonText: vmd.sharedLabels.confirmarButtonText,
                        // headerText: '¿Eliminar ' + this.instrumento.clase + '?',
                        size: 'xs',
                        headerText: vmd.sharedLabels.confirmationMessage,
                        bodyText: vmd.sharedLabels.mensajeHabilitado
                    };
                    vmd.modalService.showModal({}, modalOptions).then((result) => {
                        if (result === 'ok') {
                            vmd.habilitado();
                        }
                    });
                    break;
                case app.common.eTipoHabilitacion.InhabilitadoInelegible:
                    var modalOptions = {
                        cancelButtonText: vmd.sharedLabels.cancelButtonText,
                        actionButtonText: vmd.sharedLabels.confirmarButtonText,
                        // headerText: '¿Eliminar ' + this.instrumento.clase + '?',
                        size: 'xs',
                        headerText: vmd.sharedLabels.confirmationMessage,
                        bodyText: vmd.sharedLabels.mensajeInhabilitadoInelegible
                    };
                    vmd.modalService.showModal({}, modalOptions).then((result) => {
                        if (result === 'ok') {
                            vmd.inelegible();
                        }
                    });
                    break;
            }

        }
        private habilitado(): void {
            var vmd = this;
            vmd.appSetting.isLoading = true;
            vmd.currentInstrumentoAccion.indHabilitacionRiesgo = app.common.eTipoHabilitacion.Habilitado;
            vmd.instrumentoAccionService.activeInstrumento(vmd.currentInstrumentoAccion).then(data => {
                vmd.claimMessage.success(data);
                vmd.$uibModalInstance.close('ok');
                vmd.appSetting.isLoading = false;
            },
                (error: common.IHttpPromiseCallbackErrorArg) => { vmd.appSetting.isLoading = false; vmd.claimMessage.error(error.data); });
        }
        private inelegible(): void {
            var vmd = this;
            vmd.appSetting.isLoading = true;
            vmd.currentInstrumentoAccion.indHabilitacionRiesgo = app.common.eTipoHabilitacion.InhabilitadoInelegible;
            vmd.instrumentoAccionService.activeInstrumento(vmd.currentInstrumentoAccion).then(data => {
                vmd.claimMessage.success(data);
                vmd.$uibModalInstance.close('ok');
                vmd.appSetting.isLoading = false;
            },
                (error: common.IHttpPromiseCallbackErrorArg) => { vmd.appSetting.isLoading = false; vmd.claimMessage.error(error.data); });
        }

        changeDescripcion(): void {
            var vmd = this;
            var nombreTipoAccion = vmd.ctrlTiposAcciones.selectedItem != null ? (vmd.ctrlTiposAcciones.selectedItem.idTipoInstrumento > 0 ? vmd.ctrlTiposAcciones.selectedItem.nombreSbsTipoInstrumento : "") : "";
            var nombreEmisor = vmd.ctrlEmisores.selectedItem != null ? (vmd.ctrlEmisores.selectedItem.idEntidad > 0 ? vmd.ctrlEmisores.selectedItem.nombreEntidad : "") : "";
            vmd.currentInstrumentoAccion.descripcion = nombreTipoAccion + " - " + nombreEmisor;

            vmd.changeCodigoSbsGenerated();
        }

        changeCodigoSbsGenerated(): void {
            var vmd = this;
            if (!vmd.currentInstrumentoAccion.isAdrAds){
                var codigoSbsTipoAccion = vmd.ctrlTiposAcciones.selectedItem != null ? (vmd.ctrlTiposAcciones.selectedItem.idTipoInstrumento > 0 ? vmd.ctrlTiposAcciones.selectedItem.codigoSbsTipoInstrumento : "") : "";
                var codigoSbsEmisor = vmd.ctrlEmisores.selectedItem != null ? (vmd.ctrlEmisores.selectedItem.idEntidad > 0 ? vmd.ctrlEmisores.selectedItem.codigoSbsEmisor : "") : "";
                var codigoTipoMoneda = vmd.ctrlMonedas.selectedItem != null ? (vmd.ctrlMonedas.selectedItem.idMoneda > 0 ? vmd.ctrlMonedas.selectedItem.codigoSBS : "") : "";
                vmd.currentInstrumentoAccion.codigoSbsGenerated = <string>codigoSbsTipoAccion + vmd.setCadenaConcat(<string>codigoSbsEmisor, '-', 1) + vmd.setCadenaConcat(<string>codigoTipoMoneda, '-', 1);
                vmd.currentInstrumentoAccion.codigoSbsGeneratedBase = <string>codigoSbsTipoAccion + <string>codigoSbsEmisor + <string>codigoTipoMoneda;
            }
        }

        changeMoneda(): void {
            var vmd = this;
            vmd.changeCodigoSbsGenerated();

            var idMonedaExcluir = vmd.ctrlMonedas.selectedItem != null ? vmd.ctrlMonedas.selectedItem.idMoneda : 0;
            vmd.monedaService.getAllMonedasActiveByIdMonedaExcluir(0, vmd.sharedLabels.selectText).then(data => {
                vmd.listaDualMoneda = data;
                vmd.ctrlDualMoneda.itemsSource = vmd.listaDualMoneda;
                vmd.ctrlDualMoneda.selectedValue = vmd.idDualMoneda;
            }).catch((error) => { vmd.claimMessage.error(error.data); });
        }

        //This method is fired wen the user click the save button
        ok(): void {
            switch (this.operationType) {
                case app.common.eOperation.AddNew:
                    this.create();
                    break;
                case app.common.eOperation.Update:
                    this.update();
                    break;
                case app.common.eOperation.Annul:
                    if (this.currentInstrumentoAccion.listadoInstrumentoAccionAdsAdsDTO && this.currentInstrumentoAccion.listadoInstrumentoAccionAdsAdsDTO.length > 0) {
                        var modalOptions = {
                            cancelButtonText: this.sharedLabels.cancelButtonText,
                            actionButtonText: this.sharedLabels.acceptButtonText,
                            size: 'xs',
                            headerText: this.sharedLabels.confirmationMessage,
                            bodyText: "Este instrumento tiene instrumentos hijos, al anular se anularan tambien los instrumentos hijos. </br> ¿Desea continuar?"
                        };
                        this.modalService.showModal({}, modalOptions).then((result) => {
                            if (result === 'ok') {
                                this.disable();
                            }
                        });
                    } else {
                        this.disable();
                    }
                    break;
                case app.common.eOperation.Remove:
                    if (this.currentInstrumentoAccion.listadoInstrumentoAccionAdsAdsDTO && this.currentInstrumentoAccion.listadoInstrumentoAccionAdsAdsDTO.length > 0) {
                        var modalOptions = {
                            cancelButtonText: this.sharedLabels.cancelButtonText,
                            actionButtonText: this.sharedLabels.acceptButtonText,
                            size: 'xs',
                            headerText: this.sharedLabels.confirmationMessage,
                            bodyText: "Este instrumento tiene instrumentos hijos, al eliminar se eliminaran tambien los instrumentos hijos. </br> ¿Desea continuar?"
                        };
                        this.modalService.showModal({}, modalOptions).then((result) => {
                            if (result === 'ok') {
                                this.remove();
                            }
                        });
                    } else {
                        this.remove();
                    }
                    break;
            }
        }

        validateRequiredFields(): boolean {
            var isValid = true;
            if (!this.currentItemForm.$valid) {
                var errorMessageForRequiredFields = this.sharedLabels.requiredFields2 + "<br />";

                if (this.currentItemForm.$error.wjValidationError)
                    for (var i = 0; i < this.currentItemForm.$error.wjValidationError.length; i++)
                        errorMessageForRequiredFields = errorMessageForRequiredFields + "- " + this.currentItemForm.$error.wjValidationError[i].$name + "<br />";

                if (this.currentItemForm.$error.minlength)
                    for (var i = 0; i < this.currentItemForm.$error.minlength.length; i++)
                        errorMessageForRequiredFields = this.sharedLabels.minLengthMessage + "- " + this.currentItemForm.$error.minlength[i].$name + "<br />";
                this.claimMessage.error(errorMessageForRequiredFields);
                isValid = false;
            }
            else {
                if (this.currentInstrumentoAccion.codigoSbsJoined.length != this.customLength.longCode) {
                    this.claimMessage.error(this.sharedLabels.codigoSbsShouldHasMinLength);
                    isValid = false;
                }
                else {
                    this.currentInstrumentoAccion.loginActualizacion = this.currentUser.userId;
                    this.currentInstrumentoAccion.codigoSbs = (this.currentInstrumentoAccion.codigoSbsGeneratedBase == null ? this.currentInstrumentoAccion.codigoSbsGenerated : this.currentInstrumentoAccion.codigoSbsGeneratedBase.trim()) + this.currentInstrumentoAccion.codigoSbsJoined;
                    this.currentInstrumentoAccion.nombreInstrumento = this.ctrlTiposAcciones.selectedItem.nombreSbsTipoInstrumento + " - " + this.ctrlEmisores.selectedItem.nombreEntidad + " - " + this.currentInstrumentoAccion.codigoSbs;
                    this.currentInstrumentoAccion.secuencialFechaEmision = app.common.dateToString(this.currentInstrumentoAccion.fechaEmision);
                    this.currentInstrumentoAccion.secuencialFechaVencimiento = app.common.dateToString(this.currentInstrumentoAccion.fechaVencimiento);
                    this.currentInstrumentoAccion.secuencialFechaMontoEmitido = app.common.dateToString(this.currentInstrumentoAccion.fechaMontoEmitido);
                    this.currentInstrumentoAccion.secuencialFechaMontoColocado = app.common.dateToString(this.currentInstrumentoAccion.fechaMontoColocado);
                }
            }
            return isValid;
        }

        infoFechaVencimiento(sender, eventArgs): void {
            var vmd = this;
            if (vmd.fechaInitial) {
                if (sender.value != null) {
                    if (sender.value < vmd.currentInstrumentoAccion.fechaEmision && vmd.currentInstrumentoAccion.fechaEmision != null) {
                        vmd.claimMessage.warning("- " + vmd.sharedLabels.la + " " + vmd.instrumentoAccionLabels.fechaVencimiento + " " + vmd.sharedLabels.noPuedeSerMenorAla + " " + vmd.instrumentoAccionLabels.fechaEmision);
                        sender.value = null;
                    }
                }
            }
        }
        infoFechaInicio(sender, eventArgs): void {
            var vmd = this;
            if (vmd.fechaInitial) {
                if (sender.value != null) {
                    if (vmd.currentInstrumentoAccion.fechaVencimiento < sender.value && vmd.currentInstrumentoAccion.fechaVencimiento != null) {
                        vmd.claimMessage.warning("- " + vmd.sharedLabels.la + " " + vmd.instrumentoAccionLabels.fechaEmision + " " + vmd.sharedLabels.noPuedeSerMayorAla + " " + vmd.instrumentoAccionLabels.fechaVencimiento);
                        sender.value = null;
                    }
                }
            }
        }
        //This method  add a new item
        create(): void {
            var isValid = this.validateRequiredFields();
            if (!isValid)
                return;

            this.appSetting.isLoading = true;
            this.instrumentoAccionService.create(this.currentInstrumentoAccion).then(data => {
                this.appSetting.isLoading = false;
                this.claimMessage.success(this.sharedLabels.newMessageOK);
                this.$uibModalInstance.close('ok');
            },
                (error: common.IHttpPromiseCallbackErrorArg) => { this.appSetting.isLoading = false; this.claimMessage.error(error.data) });
        }

        //This method  update a item
        update(): void {
            var isValid = this.validateRequiredFields();
            if (!isValid)
                return;

            this.appSetting.isLoading = true;
            this.instrumentoAccionService.update(this.currentInstrumentoAccion).then(data => {
                this.appSetting.isLoading = false;
                this.claimMessage.success(this.sharedLabels.editMessageOK);
                this.$uibModalInstance.close('ok');
            },
                (error: common.IHttpPromiseCallbackErrorArg) => { this.appSetting.isLoading = false; this.claimMessage.error(error.data) });
        }

        //This method  remove the item
        remove(): void {
            this.appSetting.isLoading = true;
            this.instrumentoAccionService.remove(this.currentInstrumentoAccion.idInstrumento,
                this.currentInstrumentoAccion.idAccion).then(data => {
                    this.appSetting.isLoading = false;
                    this.claimMessage.success(this.sharedLabels.removeMessageOK);
                    this.$uibModalInstance.close('ok');
                },
                (error: common.IHttpPromiseCallbackErrorArg) => { this.appSetting.isLoading = false; this.claimMessage.error(error.data) });
        }

        //This method  annul the item
        disable(): void {
            var isValid = this.validateRequiredFields();
            if (!isValid)
                return;

            this.appSetting.isLoading = true;
            this.instrumentoAccionService.disable(this.currentInstrumentoAccion).then(data => {
                this.appSetting.isLoading = false;
                this.claimMessage.success(this.sharedLabels.disableMessageOK);
                this.$uibModalInstance.close('ok');
            },
                (error: common.IHttpPromiseCallbackErrorArg) => { this.appSetting.isLoading = false; this.claimMessage.error(error.data) });
        }

        //This method  is fired when the user close this popup window
        close(): void {
            if (this.operationType == app.common.eOperation.AddNew || this.operationType == app.common.eOperation.Update) {
                if (this.isCurrenItemDisabled) //If the current item is disabled then the system does not request to the user confirmation to leaving the window without save changes
                    this.cancel();
                else {
                    var modalOptions = {
                        cancelButtonText: this.sharedLabels.cancelButtonText,
                        actionButtonText: this.sharedLabels.acceptButtonText,
                        size: 'xs',
                        headerText: this.sharedLabels.confirmationMessage,
                        bodyText: this.sharedLabels.headerTextMessage
                    };
                    this.modalService.showModal({}, modalOptions).then((result) => {
                        if (result === 'ok') {
                            this.cancel();
                        }
                    });
                }
            };
        }

        verifyCurrentItemIsDisabled(): boolean {
            //return (this.ctrlEstados.selectedItem.descripcion == this.sharedLabels.annulValueText) ? true : false;
            var vmd = this;
            var oEstado = vmd.estados.filter(x => x.id == vmd.currentInstrumentoAccion.indActividad)[0];
            return (oEstado.descripcion == vmd.sharedLabels.annulValueText) ? true : false;
        }

        cancel(): void {
            this.$uibModalInstance.close('cancel');
        }

        loadNext(): void {
            var vmd = this;
            var currentPosition = vmd.currentItem + 1;
            vmd.setSelected(currentPosition);
        }

        loadPrevius(): void {
            var vmd = this;
            var currentPosition = vmd.currentItem - 1;
            vmd.setSelected(currentPosition);
        }

        setCadenaConcat(fullText: string, concatText: string, posisionText: number): string {
            return app.common.setCadenaConcat(fullText, concatText, posisionText);
        }

        obtenerNombreClasificacion(): string {
            var vmd = this;
            var nombre = vmd.clasificacionesRiesgoSBS.filter(x => x.idClasificacionRiesgo == vmd.currentInstrumentoAccion.idClasificacionRiesgo);

            if (nombre == null || nombre.length == 0)
                return "";
            else
            return nombre[0].descripcionClasificacionRiesgo;
        }

        //this method select current element from list
        setSelected(currentPosition: number) {
            var vmd = this;
            var current = <app.common.IPosicion>{};
            current = vmd.initialInstrumentoAccion.recorrido.filter(x => x.item == currentPosition)[0];
            if (current != null) {
                vmd.appSetting.isLoading = true;
                vmd.instrumentoAccionService.getByIdInstrumentoAccion(current.idItem, current.idSubItem).then(data => {
                    vmd.currentInstrumentoAccion = data;
                    vmd.setInstrumentoAccion();
                    vmd.currentItem = currentPosition;
                    vmd.appSetting.isLoading = false;
                },
                    (error: common.IHttpPromiseCallbackErrorArg) => { vmd.appSetting.isLoading = false; vmd.claimMessage.error(error.data) });
            }
        }

        //This method is the controller because the html needs to call it
        cleanStringOfInvalidCharacters(fullText: string): string {
            return app.common.cleanStringOfInvalidCharacters(fullText);
        }
        cleanStringOfInvalidCharactersFullCharacters(fullText: string): string {
            return app.common.cleanStringOfInvalidCharactersFullCharacters(fullText);
        }
        //This method is the controller because the html needs to call it
        stringIsNullOrEmpty(stringToEvaluate: string): boolean {
            return app.common.stringIsNullOrEmpty(stringToEvaluate);
        }

        history(): void {
            this.claimMessage.info(this.sharedLabels.functionNotYetAvaiable);
        }

        changeFechaVencimiento() {
            if (!this.currentInstrumentoAccion.tieneFechaVencimiento)
                this.currentInstrumentoAccion.fechaVencimiento = null;
        }


        definirAdr(): void {
            const vm = this;
            var modalDefaults = {
                backdrop: true,
                keyboard: true,
                modalFade: true,
                templateUrl: 'app/mesadineroApp/instrumentoAccion/definir-adr-ads/instrumento-accion-detail.html',
                controller: DefinirAdrAdsController,
                controllerAs: 'vmde',
                size: "lg",
                resolve: {
                    operationType: () => { return vm.operationType },
                    doubleClickDisabled: () => { return vm.doubleClickDisabled  },
                    currentInstrumentoAccion: () => { return vm.currentInstrumentoAccion },
                    ListadoInstrumentoAccionAdsAds: () => { return vm.currentInstrumentoAccion.listadoInstrumentoAccionAdsAdsDTO },
                    isDisabled: () => { return (vm.operationType != app.common.eOperation.AddNew && vm.operationType != app.common.eOperation.Update) || vm.doubleClickDisabled || vm.currentInstrumentoAccion.isAdrAds},
                    IdItem: () => { return null },
                    emisores: () => { return vm.emisores }
                }
            };

            //If the operation in the modal is succesful
            vm.modalService.showModal(modalDefaults, {}).then((result) => {
                if (result[0] === 'ok') {
                    vm.currentInstrumentoAccion.listadoInstrumentoAccionAdsAdsDTO = <app.common.IInstrumentoAdrAds[]>result[1];
                }
            });
        }

    }


    angular
        .module('app.mesadineroapp.instrumentoAccion')
        .controller(InstrumentoAccionDetailController.controllerId,
        InstrumentoAccionDetailController);

}