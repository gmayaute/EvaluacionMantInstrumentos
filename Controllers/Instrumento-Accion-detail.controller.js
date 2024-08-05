var app;
(function (app) {
    var mesadineroapp;
    (function (mesadineroapp) {
        var instrumentoAccion;
        (function (instrumentoAccion) {
            'use strict';
            var InstrumentoAccionDetailController = (function () {
                function InstrumentoAccionDetailController(initialInstrumentoAccion, operationType, doubleClickDisabled, totalItems, $uibModalInstance, modalService, claimMessage, $scope, $compile, $window, appSetting, sharedLabels, currentUser, customLength, $q, instrumentoAccionLabels, sharedIndicadorService, instrumentoAccionService, tipoInstrumentoService, entidadService, monedaService, clasificacionRiesgoService) {
                    this.initialInstrumentoAccion = initialInstrumentoAccion;
                    this.operationType = operationType;
                    this.doubleClickDisabled = doubleClickDisabled;
                    this.totalItems = totalItems;
                    this.$uibModalInstance = $uibModalInstance;
                    this.modalService = modalService;
                    this.claimMessage = claimMessage;
                    this.$scope = $scope;
                    this.$compile = $compile;
                    this.$window = $window;
                    this.appSetting = appSetting;
                    this.sharedLabels = sharedLabels;
                    this.currentUser = currentUser;
                    this.customLength = customLength;
                    this.$q = $q;
                    this.instrumentoAccionLabels = instrumentoAccionLabels;
                    this.sharedIndicadorService = sharedIndicadorService;
                    this.instrumentoAccionService = instrumentoAccionService;
                    this.tipoInstrumentoService = tipoInstrumentoService;
                    this.entidadService = entidadService;
                    this.monedaService = monedaService;
                    this.clasificacionRiesgoService = clasificacionRiesgoService;
                    this.tiposAcciones = [];
                    this.emisores = [];
                    this.emisoresAll = [];
                    this.monedas = [];
                    this.monedasAll = [];
                    this.listaDualMoneda = [];
                    this.estados = [];
                    this.habilitaciones = [];
                    this.tiposCustodia = [];
                    this.unidades = [];
                    this.clasificacionesRiesgoSBS = [];
                    this.focosGeografico = [];
                    this.clases = [];
                    this.regiones = [];
                    this.categorias = [];
                    this.paises = [];
                    this.familias = [];
                    this.codigoSbsIngresado = "";
                    this.idDualMoneda = 0;
                    this.fechaInitial = false;
                    this.$compile = $compile;
                    this.$scope = $scope;
                    this.$window.onhashchange = function () { $uibModalInstance.close('cancel'); };
                    this.initialize();
                }
                InstrumentoAccionDetailController.prototype.initialize = function () {
                    try {
                        var vm = this;
                        vm.isEditing = (vm.operationType > app.common.eOperation.AddNew) ? true : false; //When the operation is  Update ||Remove || Annul
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
                        vm.$scope.$watch(function () {
                            $(".wj-header").css("background-color", vm.appSetting.codigoColor);
                            $(".modal-header").css("background-color", vm.appSetting.codigoColor);
                            $(".btn_color_selecc").css("background-color", vm.appSetting.codigoColor);
                            return vm;
                        }, function (valorNuevo, valorAntiguo) { });
                    }
                    catch (e) {
                        alert(e);
                    }
                };
                InstrumentoAccionDetailController.prototype.preload = function () {
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
                        .then(function (results) {
                        vmd.tiposAcciones = results[0];
                        if (vmd.initialInstrumentoAccion.idInstrumento > 0) {
                        }
                        else {
                            var tiposAdr = results[16];
                            var tiposAds = results[17];
                            if (tiposAdr != null)
                                tiposAdr.forEach(function (value, index) {
                                    vmd.tiposAcciones = vmd.tiposAcciones.filter(function (x) { return x.idTipoInstrumento != value.idTipoInstrumento; });
                                });
                            if (tiposAds != null)
                                tiposAds.forEach(function (value, index) {
                                    vmd.tiposAcciones = vmd.tiposAcciones.filter(function (x) { return x.idTipoInstrumento != value.idTipoInstrumento; });
                                });
                        }
                        vmd.emisores = results[1];
                        vmd.monedas = results[2];
                        vmd.estados = results[3];
                        vmd.habilitaciones = results[4];
                        vmd.tiposCustodia = results[5];
                        vmd.unidades = results[6];
                        vmd.clasificacionesRiesgoSBS = results[7];
                        vmd.focosGeografico = results[8];
                        vmd.clases = results[9];
                        vmd.regiones = results[10];
                        vmd.categorias = results[11];
                        vmd.paises = results[12];
                        vmd.familias = results[13];
                        vmd.emisoresAll = results[14];
                        vmd.monedasAll = results[15];
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
                        .catch(function (error) {
                        vmd.claimMessage.error(error.data);
                        vmd.appSetting.isLoading = false;
                    });
                };
                InstrumentoAccionDetailController.prototype.loadInstrumentoAccion = function () {
                    var vm = this;
                    vm.appSetting.isLoading = true;
                    if (vm.initialInstrumentoAccion.idInstrumento > 0) {
                        vm.emisores = angular.copy(vm.emisoresAll);
                        vm.instrumentoAccionService.getByIdInstrumentoAccion(vm.initialInstrumentoAccion.idInstrumento, vm.initialInstrumentoAccion.idAccion).then(function (data) {
                            vm.currentInstrumentoAccion = data;
                            vm.setInstrumentoAccion();
                            vm.fechaInitial = true;
                            vm.appSetting.isLoading = false;
                        }, function (error) { vm.claimMessage.error(error.data); vm.appSetting.isLoading = true; });
                    }
                    else {
                        vm.fechaInitial = true;
                        vm.appSetting.isLoading = false;
                        vm.classMercadoPopUp = 'col-xs-10';
                    }
                };
                InstrumentoAccionDetailController.prototype.setInstrumentoAccion = function () {
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
                };
                InstrumentoAccionDetailController.prototype.verifyCurrentItemIsDisabledInelegible = function () {
                    var vmd = this;
                    if (vmd.currentInstrumentoAccion != undefined) {
                        return vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(function (x) { return x.idIndicador == app.common.eEstado.Anulado; })[0].id ? false :
                            (vmd.currentInstrumentoAccion.indHabilitacionRiesgo == vmd.habilitaciones.filter(function (x) { return x.idIndicador == app.common.eTipoHabilitacion.Habilitado; })[0].id &&
                                vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(function (x) { return x.idIndicador == app.common.eEstado.Vigente; })[0].id) ? false :
                                (vmd.currentInstrumentoAccion.indHabilitacionRiesgo == vmd.habilitaciones.filter(function (x) { return x.idIndicador == app.common.eTipoHabilitacion.InhabilitadoInelegible; })[0].id &&
                                    vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(function (x) { return x.idIndicador == app.common.eEstado.Vigente; })[0].id) ? true :
                                    (vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(function (x) { return x.idIndicador == app.common.eEstado.Vigente; })[0].id) ? true : false;
                    }
                    return false;
                };
                InstrumentoAccionDetailController.prototype.verifyCurrentItemIsDisabledHabilitado = function () {
                    var vmd = this;
                    if (vmd.currentInstrumentoAccion != undefined) {
                        return vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(function (x) { return x.idIndicador == app.common.eEstado.Anulado; })[0].id ? false :
                            (vmd.currentInstrumentoAccion.indHabilitacionRiesgo == vmd.habilitaciones.filter(function (x) { return x.idIndicador == app.common.eTipoHabilitacion.Habilitado; })[0].id &&
                                vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(function (x) { return x.idIndicador == app.common.eEstado.Vigente; })[0].id) ? true :
                                (vmd.currentInstrumentoAccion.indHabilitacionRiesgo == vmd.habilitaciones.filter(function (x) { return x.idIndicador == app.common.eTipoHabilitacion.InhabilitadoInelegible; })[0].id &&
                                    vmd.currentInstrumentoAccion.indActividad == vmd.estados.filter(function (x) { return x.idIndicador == app.common.eEstado.Vigente; })[0].id) ? true : false;
                    }
                    return false;
                };
                InstrumentoAccionDetailController.prototype.cambiarEstado = function (id) {
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
                            vmd.modalService.showModal({}, modalOptions).then(function (result) {
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
                            vmd.modalService.showModal({}, modalOptions).then(function (result) {
                                if (result === 'ok') {
                                    vmd.inelegible();
                                }
                            });
                            break;
                    }
                };
                InstrumentoAccionDetailController.prototype.habilitado = function () {
                    var vmd = this;
                    vmd.appSetting.isLoading = true;
                    vmd.currentInstrumentoAccion.indHabilitacionRiesgo = app.common.eTipoHabilitacion.Habilitado;
                    vmd.instrumentoAccionService.activeInstrumento(vmd.currentInstrumentoAccion).then(function (data) {
                        vmd.claimMessage.success(data);
                        vmd.$uibModalInstance.close('ok');
                        vmd.appSetting.isLoading = false;
                    }, function (error) { vmd.appSetting.isLoading = false; vmd.claimMessage.error(error.data); });
                };
                InstrumentoAccionDetailController.prototype.inelegible = function () {
                    var vmd = this;
                    vmd.appSetting.isLoading = true;
                    vmd.currentInstrumentoAccion.indHabilitacionRiesgo = app.common.eTipoHabilitacion.InhabilitadoInelegible;
                    vmd.instrumentoAccionService.activeInstrumento(vmd.currentInstrumentoAccion).then(function (data) {
                        vmd.claimMessage.success(data);
                        vmd.$uibModalInstance.close('ok');
                        vmd.appSetting.isLoading = false;
                    }, function (error) { vmd.appSetting.isLoading = false; vmd.claimMessage.error(error.data); });
                };
                InstrumentoAccionDetailController.prototype.changeDescripcion = function () {
                    var vmd = this;
                    var nombreTipoAccion = vmd.ctrlTiposAcciones.selectedItem != null ? (vmd.ctrlTiposAcciones.selectedItem.idTipoInstrumento > 0 ? vmd.ctrlTiposAcciones.selectedItem.nombreSbsTipoInstrumento : "") : "";
                    var nombreEmisor = vmd.ctrlEmisores.selectedItem != null ? (vmd.ctrlEmisores.selectedItem.idEntidad > 0 ? vmd.ctrlEmisores.selectedItem.nombreEntidad : "") : "";
                    vmd.currentInstrumentoAccion.descripcion = nombreTipoAccion + " - " + nombreEmisor;
                    vmd.changeCodigoSbsGenerated();
                };
                InstrumentoAccionDetailController.prototype.changeCodigoSbsGenerated = function () {
                    var vmd = this;
                    if (!vmd.currentInstrumentoAccion.isAdrAds) {
                        var codigoSbsTipoAccion = vmd.ctrlTiposAcciones.selectedItem != null ? (vmd.ctrlTiposAcciones.selectedItem.idTipoInstrumento > 0 ? vmd.ctrlTiposAcciones.selectedItem.codigoSbsTipoInstrumento : "") : "";
                        var codigoSbsEmisor = vmd.ctrlEmisores.selectedItem != null ? (vmd.ctrlEmisores.selectedItem.idEntidad > 0 ? vmd.ctrlEmisores.selectedItem.codigoSbsEmisor : "") : "";
                        var codigoTipoMoneda = vmd.ctrlMonedas.selectedItem != null ? (vmd.ctrlMonedas.selectedItem.idMoneda > 0 ? vmd.ctrlMonedas.selectedItem.codigoSBS : "") : "";
                        vmd.currentInstrumentoAccion.codigoSbsGenerated = codigoSbsTipoAccion + vmd.setCadenaConcat(codigoSbsEmisor, '-', 1) + vmd.setCadenaConcat(codigoTipoMoneda, '-', 1);
                        vmd.currentInstrumentoAccion.codigoSbsGeneratedBase = codigoSbsTipoAccion + codigoSbsEmisor + codigoTipoMoneda;
                    }
                };
                InstrumentoAccionDetailController.prototype.changeMoneda = function () {
                    var vmd = this;
                    vmd.changeCodigoSbsGenerated();
                    var idMonedaExcluir = vmd.ctrlMonedas.selectedItem != null ? vmd.ctrlMonedas.selectedItem.idMoneda : 0;
                    vmd.monedaService.getAllMonedasActiveByIdMonedaExcluir(0, vmd.sharedLabels.selectText).then(function (data) {
                        vmd.listaDualMoneda = data;
                        vmd.ctrlDualMoneda.itemsSource = vmd.listaDualMoneda;
                        vmd.ctrlDualMoneda.selectedValue = vmd.idDualMoneda;
                    }).catch(function (error) { vmd.claimMessage.error(error.data); });
                };
                //This method is fired wen the user click the save button
                InstrumentoAccionDetailController.prototype.ok = function () {
                    var _this = this;
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
                                this.modalService.showModal({}, modalOptions).then(function (result) {
                                    if (result === 'ok') {
                                        _this.disable();
                                    }
                                });
                            }
                            else {
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
                                this.modalService.showModal({}, modalOptions).then(function (result) {
                                    if (result === 'ok') {
                                        _this.remove();
                                    }
                                });
                            }
                            else {
                                this.remove();
                            }
                            break;
                    }
                };
                InstrumentoAccionDetailController.prototype.validateRequiredFields = function () {
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
                };
                InstrumentoAccionDetailController.prototype.infoFechaVencimiento = function (sender, eventArgs) {
                    var vmd = this;
                    if (vmd.fechaInitial) {
                        if (sender.value != null) {
                            if (sender.value < vmd.currentInstrumentoAccion.fechaEmision && vmd.currentInstrumentoAccion.fechaEmision != null) {
                                vmd.claimMessage.warning("- " + vmd.sharedLabels.la + " " + vmd.instrumentoAccionLabels.fechaVencimiento + " " + vmd.sharedLabels.noPuedeSerMenorAla + " " + vmd.instrumentoAccionLabels.fechaEmision);
                                sender.value = null;
                            }
                        }
                    }
                };
                InstrumentoAccionDetailController.prototype.infoFechaInicio = function (sender, eventArgs) {
                    var vmd = this;
                    if (vmd.fechaInitial) {
                        if (sender.value != null) {
                            if (vmd.currentInstrumentoAccion.fechaVencimiento < sender.value && vmd.currentInstrumentoAccion.fechaVencimiento != null) {
                                vmd.claimMessage.warning("- " + vmd.sharedLabels.la + " " + vmd.instrumentoAccionLabels.fechaEmision + " " + vmd.sharedLabels.noPuedeSerMayorAla + " " + vmd.instrumentoAccionLabels.fechaVencimiento);
                                sender.value = null;
                            }
                        }
                    }
                };
                //This method  add a new item
                InstrumentoAccionDetailController.prototype.create = function () {
                    var _this = this;
                    var isValid = this.validateRequiredFields();
                    if (!isValid)
                        return;
                    this.appSetting.isLoading = true;
                    this.instrumentoAccionService.create(this.currentInstrumentoAccion).then(function (data) {
                        _this.appSetting.isLoading = false;
                        _this.claimMessage.success(_this.sharedLabels.newMessageOK);
                        _this.$uibModalInstance.close('ok');
                    }, function (error) { _this.appSetting.isLoading = false; _this.claimMessage.error(error.data); });
                };
                //This method  update a item
                InstrumentoAccionDetailController.prototype.update = function () {
                    var _this = this;
                    var isValid = this.validateRequiredFields();
                    if (!isValid)
                        return;
                    this.appSetting.isLoading = true;
                    this.instrumentoAccionService.update(this.currentInstrumentoAccion).then(function (data) {
                        _this.appSetting.isLoading = false;
                        _this.claimMessage.success(_this.sharedLabels.editMessageOK);
                        _this.$uibModalInstance.close('ok');
                    }, function (error) { _this.appSetting.isLoading = false; _this.claimMessage.error(error.data); });
                };
                //This method  remove the item
                InstrumentoAccionDetailController.prototype.remove = function () {
                    var _this = this;
                    this.appSetting.isLoading = true;
                    this.instrumentoAccionService.remove(this.currentInstrumentoAccion.idInstrumento, this.currentInstrumentoAccion.idAccion).then(function (data) {
                        _this.appSetting.isLoading = false;
                        _this.claimMessage.success(_this.sharedLabels.removeMessageOK);
                        _this.$uibModalInstance.close('ok');
                    }, function (error) { _this.appSetting.isLoading = false; _this.claimMessage.error(error.data); });
                };
                //This method  annul the item
                InstrumentoAccionDetailController.prototype.disable = function () {
                    var _this = this;
                    var isValid = this.validateRequiredFields();
                    if (!isValid)
                        return;
                    this.appSetting.isLoading = true;
                    this.instrumentoAccionService.disable(this.currentInstrumentoAccion).then(function (data) {
                        _this.appSetting.isLoading = false;
                        _this.claimMessage.success(_this.sharedLabels.disableMessageOK);
                        _this.$uibModalInstance.close('ok');
                    }, function (error) { _this.appSetting.isLoading = false; _this.claimMessage.error(error.data); });
                };
                //This method  is fired when the user close this popup window
                InstrumentoAccionDetailController.prototype.close = function () {
                    var _this = this;
                    if (this.operationType == app.common.eOperation.AddNew || this.operationType == app.common.eOperation.Update) {
                        if (this.isCurrenItemDisabled)
                            this.cancel();
                        else {
                            var modalOptions = {
                                cancelButtonText: this.sharedLabels.cancelButtonText,
                                actionButtonText: this.sharedLabels.acceptButtonText,
                                size: 'xs',
                                headerText: this.sharedLabels.confirmationMessage,
                                bodyText: this.sharedLabels.headerTextMessage
                            };
                            this.modalService.showModal({}, modalOptions).then(function (result) {
                                if (result === 'ok') {
                                    _this.cancel();
                                }
                            });
                        }
                    }
                    ;
                };
                InstrumentoAccionDetailController.prototype.verifyCurrentItemIsDisabled = function () {
                    //return (this.ctrlEstados.selectedItem.descripcion == this.sharedLabels.annulValueText) ? true : false;
                    var vmd = this;
                    var oEstado = vmd.estados.filter(function (x) { return x.id == vmd.currentInstrumentoAccion.indActividad; })[0];
                    return (oEstado.descripcion == vmd.sharedLabels.annulValueText) ? true : false;
                };
                InstrumentoAccionDetailController.prototype.cancel = function () {
                    this.$uibModalInstance.close('cancel');
                };
                InstrumentoAccionDetailController.prototype.loadNext = function () {
                    var vmd = this;
                    var currentPosition = vmd.currentItem + 1;
                    vmd.setSelected(currentPosition);
                };
                InstrumentoAccionDetailController.prototype.loadPrevius = function () {
                    var vmd = this;
                    var currentPosition = vmd.currentItem - 1;
                    vmd.setSelected(currentPosition);
                };
                InstrumentoAccionDetailController.prototype.setCadenaConcat = function (fullText, concatText, posisionText) {
                    return app.common.setCadenaConcat(fullText, concatText, posisionText);
                };
                InstrumentoAccionDetailController.prototype.obtenerNombreClasificacion = function () {
                    var vmd = this;
                    var nombre = vmd.clasificacionesRiesgoSBS.filter(function (x) { return x.idClasificacionRiesgo == vmd.currentInstrumentoAccion.idClasificacionRiesgo; });
                    if (nombre == null || nombre.length == 0)
                        return "";
                    else
                        return nombre[0].descripcionClasificacionRiesgo;
                };
                //this method select current element from list
                InstrumentoAccionDetailController.prototype.setSelected = function (currentPosition) {
                    var vmd = this;
                    var current = {};
                    current = vmd.initialInstrumentoAccion.recorrido.filter(function (x) { return x.item == currentPosition; })[0];
                    if (current != null) {
                        vmd.appSetting.isLoading = true;
                        vmd.instrumentoAccionService.getByIdInstrumentoAccion(current.idItem, current.idSubItem).then(function (data) {
                            vmd.currentInstrumentoAccion = data;
                            vmd.setInstrumentoAccion();
                            vmd.currentItem = currentPosition;
                            vmd.appSetting.isLoading = false;
                        }, function (error) { vmd.appSetting.isLoading = false; vmd.claimMessage.error(error.data); });
                    }
                };
                //This method is the controller because the html needs to call it
                InstrumentoAccionDetailController.prototype.cleanStringOfInvalidCharacters = function (fullText) {
                    return app.common.cleanStringOfInvalidCharacters(fullText);
                };
                InstrumentoAccionDetailController.prototype.cleanStringOfInvalidCharactersFullCharacters = function (fullText) {
                    return app.common.cleanStringOfInvalidCharactersFullCharacters(fullText);
                };
                //This method is the controller because the html needs to call it
                InstrumentoAccionDetailController.prototype.stringIsNullOrEmpty = function (stringToEvaluate) {
                    return app.common.stringIsNullOrEmpty(stringToEvaluate);
                };
                InstrumentoAccionDetailController.prototype.history = function () {
                    this.claimMessage.info(this.sharedLabels.functionNotYetAvaiable);
                };
                InstrumentoAccionDetailController.prototype.changeFechaVencimiento = function () {
                    if (!this.currentInstrumentoAccion.tieneFechaVencimiento)
                        this.currentInstrumentoAccion.fechaVencimiento = null;
                };
                InstrumentoAccionDetailController.prototype.definirAdr = function () {
                    var vm = this;
                    var modalDefaults = {
                        backdrop: true,
                        keyboard: true,
                        modalFade: true,
                        templateUrl: 'app/mesadineroApp/instrumentoAccion/definir-adr-ads/instrumento-accion-detail.html',
                        controller: instrumentoAccion.DefinirAdrAdsController,
                        controllerAs: 'vmde',
                        size: "lg",
                        resolve: {
                            operationType: function () { return vm.operationType; },
                            doubleClickDisabled: function () { return vm.doubleClickDisabled; },
                            currentInstrumentoAccion: function () { return vm.currentInstrumentoAccion; },
                            ListadoInstrumentoAccionAdsAds: function () { return vm.currentInstrumentoAccion.listadoInstrumentoAccionAdsAdsDTO; },
                            isDisabled: function () { return (vm.operationType != app.common.eOperation.AddNew && vm.operationType != app.common.eOperation.Update) || vm.doubleClickDisabled || vm.currentInstrumentoAccion.isAdrAds; },
                            IdItem: function () { return null; },
                            emisores: function () { return vm.emisores; }
                        }
                    };
                    //If the operation in the modal is succesful
                    vm.modalService.showModal(modalDefaults, {}).then(function (result) {
                        if (result[0] === 'ok') {
                            vm.currentInstrumentoAccion.listadoInstrumentoAccionAdsAdsDTO = result[1];
                        }
                    });
                };
                InstrumentoAccionDetailController.controllerId = 'app.mesadineroapp.instrumentoAccion.instrumentoAccionDetailController';
                InstrumentoAccionDetailController.$inject = [
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
                return InstrumentoAccionDetailController;
            }());
            instrumentoAccion.InstrumentoAccionDetailController = InstrumentoAccionDetailController;
            angular
                .module('app.mesadineroapp.instrumentoAccion')
                .controller(InstrumentoAccionDetailController.controllerId, InstrumentoAccionDetailController);
        })(instrumentoAccion = mesadineroapp.instrumentoAccion || (mesadineroapp.instrumentoAccion = {}));
    })(mesadineroapp = app.mesadineroapp || (app.mesadineroapp = {}));
})(app || (app = {}));
//# sourceMappingURL=instrumento-accion-detail.controller.js.map