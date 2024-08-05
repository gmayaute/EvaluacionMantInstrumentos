module app.mesadineroapp.instrumentoAccion {
    'use strict';   
    export interface IInstrumentoAccionListScope {
        instrumentoAccion: app.common.IInstrumentoAccion;
        pagination: app.common.IPaged;
        instrumentoAccionCollection: wijmo.collections.CollectionView;
        instrumentoAccionFlexGrid: wijmo.grid.FlexGrid;
        itemsPerPage: number[];
        ctrlItemsPerPage: wijmo.input.ComboBox;
        counterFormatter: any;
        wijmoCVExportar: wijmo.collections.CollectionView;
        wijmoFlexGridExportar: wijmo.grid.FlexGrid;
        tipoExportadoGrilla: app.common.eTipoExportado;
        tiposAcciones: app.common.ITipoInstrumento[];
        emisores: app.common.IEntidadRol[];
        monedas: app.common.IMoneda[];
        estados: app.common.IIndicador[];
        habilitaciones: app.common.IIndicador[];
        previousFilter: app.common.IInstrumentoAccionFilter;
        currentFilter: app.common.IInstrumentoAccionFilter;
        filterClass: string;
        numberPages: number[];
        ctrlNumberPages: wijmo.input.ComboBox;
        userPermissions: app.common.IPermission;
        messageNoResultToShow: string;
        filterWidth: string;
    }

    export class InstrumentoAccionListController implements IInstrumentoAccionListScope {
        public static controllerId: string = 'app.mesadineroapp.instrumentoAccion.InstrumentoAccionListController';
        instrumentoAccion: app.common.IInstrumentoAccion;
        pagination: app.common.IPaged;
        instrumentoAccionCollection: wijmo.collections.CollectionView;
        instrumentoAccionFlexGrid: wijmo.grid.FlexGrid;
        itemsPerPage: number[];
        ctrlItemsPerPage: wijmo.input.ComboBox;
        counterFormatter: any;
        wijmoCVExportar: wijmo.collections.CollectionView;
        wijmoFlexGridExportar: wijmo.grid.FlexGrid;
        tipoExportadoGrilla: app.common.eTipoExportado;
        tiposAcciones: app.common.ITipoInstrumento[] = [];
        emisores: app.common.IEntidadRol[];
        monedas: app.common.IMoneda[];
        estados: app.common.IIndicador[];
        habilitaciones: app.common.IIndicador[];
        previousFilter: app.common.IInstrumentoAccionFilter;
        currentFilter: app.common.IInstrumentoAccionFilter;
        filterClass: string;
        numberPages: number[];
        ctrlNumberPages: wijmo.input.ComboBox;
        userPermissions: app.common.IPermission;
        messageNoResultToShow: string;
        filterWidth: string;
        filterStatus: boolean;
        estadoVigente: number;
        isDataPagedLoaded: boolean = false;

        static $inject = [
            'app.services.InstrumentoAccionService',
            'app.blocks.ModalService',
            'toastr',
            '$scope',
            'app.services.SharedIndicadorService',
            '$window',
            'appSetting',
            'instrumentoAccionLabels',
            'sharedLabels',
            'app.services.UsuarioService',
            'app.services.TipoInstrumentoService',
            'app.services.EntidadService',
            'app.services.MonedaService',
            '$q'
        ];
        constructor(
            private instrumentoAccionService: app.services.IInstrumentoAccionService,
            private modalService: app.blocks.IModalService,
            private claimMessage: toastr.IToastrService,
            private $scope: ng.IScope,
            private indicadorService: app.services.ISharedIndicadorService,
            private $window: ng.IWindowService,
            private appSetting: IAppSetting,
            private instrumentoAccionLabels: app.InstrumentoAccionLabels,
            private sharedLabels: SharedLabels,
            private usuarioService: app.services.IUsuarioService,
            private tipoInstrumentoService: app.services.ITipoInstrumentoService,
            private entidadService: app.services.IEntidadService,
            private monedaService: app.services.IMonedaService,
            private $q: ng.IQService
        ) {
            var vm = this;
            this.$scope = $scope;
            this.initialize();
        }

        initialize() {
            var vm = this;
            vm.filterStatus = false;
            vm.initializeThemeColor();

            this.itemsPerPage = [5, 10, 15, 20, 25];

            this.pagination = <app.common.IPaged>{
                currentPage: 1,
                itemsPerPage: this.itemsPerPage[2],
                orderColumn: "_",
                isAscending: true
            }
            vm.indicadorService.getId(app.common.eIndicador.Estado, app.common.eEstado.Vigente).then(data => {
                vm.estadoVigente = data;
                // To initialize current items
                vm.previousFilter = <app.common.IInstrumentoAccionFilter>{
                    codigoSbs: "",
                    codigoIsin: "",
                    tipoAccionSelected: 0,
                    emisorSelected: 0,
                    monedaSelected: 0,
                    estadoSelected: vm.estadoVigente,
                    habilitacionSelected: 0
                }

                vm.preLoad();
            },
                (error: common.IHttpPromiseCallbackErrorArg) => { vm.appSetting.isLoading = false; vm.claimMessage.error(error.data); });
            //Load the Select of itemsPerPage
            var ctrlItemsPerPage = vm.ctrlItemsPerPage;
            if (ctrlItemsPerPage) {
                ctrlItemsPerPage.selectedIndexChanged.addHandler((handler: wijmo.IEventHandler, self: any) => {
                    vm.changeRowsPerPage();
                });
            }

            vm.onLoadFlexGrid()

            vm.$scope.$watch(() => {
                $(".prueba_demo").css("background-color", vm.appSetting.codigoColor);
            });

            vm.$scope.$on('namePublished', function (event, args) {
                console.log(args);
                this.$scope.childNameContainer = args.studentName;
            });

        }

        initializeThemeColor(): void {
            this.filterClass = 'control-sidebar control-sidebar-dark ng-scope';
            $(".panel-heading").css("background-color", this.appSetting.codigoColor);
            $(".panel-heading").css("background-color", this.appSetting.codigoColor);
            $(".skin-blue .sidebar-menu>li.active.fondo-pension>a").css("background-color", this.appSetting.codigoColor);
            $(".btn_siguiente").css("background-color", this.appSetting.codigoColor);
            $(".btn_ultimo").css("background-color", this.appSetting.codigoColor);
            $(".btn_anterior").css("background-color", this.appSetting.codigoColor);
            $(".btn_primero").css("background-color", this.appSetting.codigoColor);
            $(".franja_paginacion").css("background-color", this.appSetting.codigoColor);
            $(".alert-error").css("background-color", this.appSetting.codigoColor);
            $(".skin-blue .main-header .logo").css("background-color", this.appSetting.codigoColor);
            $(".skin-blue .main-header .navbar").css("background-color", this.appSetting.codigoColor);
            $(".franja_blue_header").css("background-color", this.appSetting.codigoColor);
            //cabecera de la grilla
            $(".wj-header").css("background-color", this.appSetting.codigoColor);
            $(".btn_nuevo_tm").css("background-color", this.appSetting.codigoColor);
            $(".btn_editar_tm").css("background-color", this.appSetting.codigoColor);
            $(".btn_eliminar_tm").css("background-color", this.appSetting.codigoColor);
            $(".btn_anular_tm").css("background-color", this.appSetting.codigoColor);
            $(".btn_cambio_vigencia_tm").css("background-color", this.appSetting.codigoColor);
            $(".miga_pan p").css("color", this.appSetting.codigoColor);
            $(".side-nav li a").hover(function () {
                $(this).css("background-color", this.appSetting.codigoColor);
            }, function () {
                $(this).css("background-color", "transparent");
            });
            $(".btn_buscar").css("background-color", this.appSetting.codigoColor);
            $(".btn_pdf").css("background-color", this.appSetting.codigoColor);
            $(".btn_excel").css("background-color", this.appSetting.codigoColor);
            $(".btn_refresh").css("background-color", this.appSetting.codigoColor);
        }

        preLoad(): void {
            var vm = this;


            var emisoresPromise = vm.entidadService.getActiveAllByRol(app.common.eEntidad.RentaFija, vm.sharedLabels.allText);
            var monedasPromise = vm.monedaService.getAll(vm.sharedLabels.allText);
            var estadosPromise = vm.indicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.Estado, vm.sharedLabels.allText);
            var habilitacionesPromise = vm.indicadorService.getAllByTipoIndicadorAndActive(app.common.eIndicador.TipoHabilitacion, vm.sharedLabels.allText);
            var itemsDataPagedPromise = vm.instrumentoAccionService.getFilteredData(vm.pagination, vm.getPreparedFilter());
            var permisosPromise = vm.usuarioService.getUserPermissions(app.common.eMesaDinero.InstrumentoAccion);
            var tiposInstrumentoPromise = vm.tipoInstrumentoService.GetAllByGrupoInstrumento(app.common.eGrupoInstrumento.Acciones, vm.sharedLabels.allText);

            vm.appSetting.isLoading = true;
            vm.$q.all([tiposInstrumentoPromise, emisoresPromise, monedasPromise, estadosPromise,
                habilitacionesPromise, itemsDataPagedPromise, permisosPromise]).then((result) => {
                    vm.tiposAcciones = <app.common.ITipoInstrumento[]>result[0];
                    vm.emisores = <app.common.IEntidadRol[]>result[1];
                    vm.monedas = <app.common.IMoneda[]>result[2];
                    vm.estados = <app.common.IIndicador[]>result[3];
                    vm.habilitaciones = <app.common.IIndicador[]>result[4];
                    vm.setResultData(<common.IInstrumentoAccionPaged>result[5], true);
                    vm.userPermissions = <app.common.IPermission>result[6];
                    vm.isDataPagedLoaded = true;
                    vm.appSetting.isLoading = false;
                }).catch((error) => {
                    vm.claimMessage.error(error.data);
                    vm.appSetting.isLoading = false;
                })
        }

        //This method set the formatter for the counter of items on the grid and the arrowicon for sortering
        onLoadFlexGrid(): void {
            var vm = this;
            vm.counterFormatter = function (panel: any, row: any, c: any, cell: any): void {
                if (panel.cellType == wijmo.grid.CellType.RowHeader) {
                    cell.textContent = (vm.pagination.itemsPerPage * (vm.pagination.currentPage - 1) + row + 1).toString();
                }
                if (panel.cellType == wijmo.grid.CellType.ColumnHeader) {
                    var col = panel.columns[c],
                        html = cell.innerHTML;
                    if (col.binding === vm.pagination.orderColumn) {
                        if (vm.pagination.isAscending)
                            html = col.header + ' <span class="wj-glyph-up"></span>';
                        else
                            html = col.header + ' <span class="wj-glyph-down"></span>';
                    }
                    else {
                        html = col.header;
                    }
                    cell.innerHTML = html;
                    cell.style.textAlign = 'center';
                    cell.style.background = vm.appSetting.codigoColor;
                }
            }
            var flexGridExportar = vm.wijmoFlexGridExportar;
            if (flexGridExportar) {
                flexGridExportar.updatedView.addHandler((handler: wijmo.IEventHandler, self: any) => {
                    switch (vm.tipoExportadoGrilla) {
                        case app.common.eTipoExportado.Excel:
                            vm.tipoExportadoGrilla = app.common.eTipoExportado.Ninguno;
                            if (!flexGridExportar.collectionView.isEmpty) {
                                wijmo.grid.xlsx.FlexGridXlsxConverter.save(flexGridExportar, { includeColumnHeaders: true, includeCellStyles: false, sheetName: vm.instrumentoAccionLabels.title }, vm.instrumentoAccionLabels.title + '.xlsx');
                            }
                            else
                                vm.claimMessage.info(vm.sharedLabels.exportXlsErrorText);
                            break;
                        case app.common.eTipoExportado.PDF:
                            vm.tipoExportadoGrilla = app.common.eTipoExportado.Ninguno;
                            if (!flexGridExportar.collectionView.isEmpty) {
                                var settingsPDF: wijmo.grid.IFlexGridExportSettings = <wijmo.grid.IFlexGridExportSettings>{
                                    exportMode: wijmo.grid.ExportMode.All,
                                    scaleMode: wijmo.grid.ScaleMode.PageWidth,
                                    documentOptions: {
                                        pageSettings: {
                                            layout: wijmo.pdf.PdfPageOrientation.Landscape
                                        },
                                        header: {
                                            text: '&[Page]\\&[Pages]\t' + vm.instrumentoAccionLabels.title + '\t&[Page]\\&[Pages]'
                                        },
                                        footer: {
                                            text: '&[Page]\\&[Pages]\t' + vm.instrumentoAccionLabels.title + '\t&[Page]\\&[Pages]'
                                        }
                                    },
                                    styles: {
                                        cellStyle: {
                                            backgroundColor: '#ffffff',
                                            borderColor: '#c6c6c6',
                                            font: new wijmo.pdf.PdfFont("Helvetica Neue, Helvetica, Arial, sans-serif"),
                                            padding: '3px'
                                        },
                                        altCellStyle: {
                                            backgroundColor: '#f9f9f9',
                                            padding: '3px'
                                        },
                                        headerCellStyle: {
                                            backgroundColor: vm.appSetting.codigoColor,
                                            color: '#fff',
                                            textAlign: 'center',
                                            paddingTop: '5px'
                                        }
                                    }
                                };
                                wijmo.grid.FlexGridPdfConverter.export(flexGridExportar, vm.instrumentoAccionLabels.title + '.pdf', settingsPDF);
                            }
                            else
                                vm.claimMessage.info(vm.sharedLabels.exportPdfErrorText);
                            break;
                    }
                });
            }
        }

        //This method set the collectionView data of the grid with de data paged
        getInstrumentoAccionPaged(loadPageNumber: boolean, loadSearch: boolean = false): void {
            var vm = this;
            vm.appSetting.isLoading = true;
            if (loadSearch)
                vm.previousFilter = angular.copy(vm.currentFilter);

            vm.instrumentoAccionService.getFilteredData(vm.pagination, vm.getPreparedFilter()).then(data => {
                vm.isDataPagedLoaded = true;
                vm.setResultData(data, loadPageNumber);
                vm.appSetting.isLoading = false;
            },
                (error: common.IHttpPromiseCallbackErrorArg) => { vm.appSetting.isLoading = false; vm.claimMessage.error(error.data); });
        }

        setResultData(data: app.common.IInstrumentoAccionPaged, loadPageNumber: boolean): void {
            this.instrumentoAccionCollection = new wijmo.collections.CollectionView(data.listaInstrumentoAccion);
            this.pagination.totalItems = data.pagination.totalItems;
            this.pagination.totalPages = data.pagination.totalPages;
            if (loadPageNumber) {
                this.numberPages = [];
                for (var i = 0; i < this.pagination.totalPages; i++) { this.numberPages.push(i + 1); }
            }
            if (this.pagination.totalPages == 0) {
                this.messageNoResultToShow = this.sharedLabels.messageNoResultToShow;
            }
        }

        //This method change the amount of items per page and and shows the first page 
        changeRowsPerPage(): void {
            var vm = this;
            if (vm.isDataPagedLoaded) {
                vm.pagination.currentPage = 1;
                vm.getInstrumentoAccionPaged(true);
            }
        }

        //This method change the page of the grid
        getPaged(event: Event): void {
            var dataAction: string = angular.element(event.currentTarget).attr('data-action').trim().toLowerCase();
            if (dataAction) {
                switch (dataAction) {
                    case 'fast-backward':
                        this.pagination.currentPage = 1;
                        break;
                    case 'step-backward':
                        this.pagination.currentPage = this.pagination.currentPage - 1;
                        break;
                    case 'step-forward':
                        this.pagination.currentPage = this.pagination.currentPage + 1;
                        break;
                    case 'fast-forward':
                        this.pagination.currentPage = this.pagination.totalPages;
                        break;
                }
                this.getInstrumentoAccionPaged(false);
            }
        }

        //This method change the page
        changePage(): void {
            var vm = this;
            if (vm.isDataPagedLoaded) vm.getInstrumentoAccionPaged(false);
        }

        //This method change the order column
        changeOrder(sender: any, eventArgs: any): void {
            var orderColumn = sender.columns[eventArgs.col].binding;

            if (this.pagination.orderColumn == sender.columns[eventArgs.col].binding) {
                this.pagination.isAscending = !this.pagination.isAscending;
            } else {
                this.pagination.orderColumn = sender.columns[eventArgs.col].binding;
                this.pagination.isAscending = true;
            }
            this.getInstrumentoAccionPaged(false);
            eventArgs.cancel = true;
        }

        //Filter methods
        cleanFilter() {
            this.currentFilter.codigoSbs = "";
            this.currentFilter.emisorSelected = 0;
            this.currentFilter.estadoSelected = this.estadoVigente;
            this.currentFilter.habilitacionSelected = 0;
            this.currentFilter.monedaSelected = 0;
            this.currentFilter.tipoAccionSelected = 0;
        }

        //reset the values of the filter
        resetValues(): void {
            this.currentFilter = angular.copy(this.previousFilter);
            this.filterStatus = !this.filterStatus;
            this.filterWidth = (this.filterStatus) ? '400px' : '0px';
        }

        search() {
            this.pagination.currentPage = 1;
            this.getInstrumentoAccionPaged(true, true);
        }

        exportToPdf(): void {
            var vm = this;
            if (vm.pagination.totalPages == 0)
                vm.claimMessage.info(vm.sharedLabels.exportPdfErrorText);
            else
                vm.listDataToExport(app.common.eTipoExportado.PDF);
        }

        exportToExcel(): void {
            var vm = this;
            if (vm.pagination.totalPages == 0)
                vm.claimMessage.info(vm.sharedLabels.exportXlsErrorText);
            else
                vm.listDataToExport(app.common.eTipoExportado.Excel);
        }

        listDataToExport(typeToExport: app.common.eTipoExportado): void {
            var vm = this;

            //vm.previousFilter = angular.copy(vm.currentFilter);

            if (typeToExport !== app.common.eTipoExportado.Ninguno) {
                vm.appSetting.isLoading = true;
                var listaInstrumentoAccionPaginado: ng.IPromise<app.common.IInstrumentoAccionPaged>;
                listaInstrumentoAccionPaginado = vm.instrumentoAccionService.getFilteredData(app.common.getPagintationForExport(vm.pagination), vm.getPreparedFilter());
                listaInstrumentoAccionPaginado.then((response: app.common.IInstrumentoAccionPaged): void => {
                    vm.tipoExportadoGrilla = typeToExport;
                    vm.wijmoCVExportar = new wijmo.collections.CollectionView(response.listaInstrumentoAccion);
                    vm.onLoadFlexGrid();
                    vm.appSetting.isLoading = false;
                },
                    (error: common.IHttpPromiseCallbackErrorArg) => { vm.appSetting.isLoading = false; vm.claimMessage.error(error.data); });
            }
        }

        getPreparedFilter(): app.common.IInstrumentoAccionFilter {
            return <app.common.IInstrumentoAccionFilter>{
                codigoSbs: app.common.validateFilterText(this.previousFilter.codigoSbs, this.sharedLabels.allText),
                codigoIsin: "_",
                tipoAccionSelected: (!this.previousFilter.tipoAccionSelected) ? 0 : this.previousFilter.tipoAccionSelected,
                emisorSelected: (!this.previousFilter.emisorSelected) ? 0 : this.previousFilter.emisorSelected,
                monedaSelected: (!this.previousFilter.monedaSelected) ? 0 : this.previousFilter.monedaSelected,
                estadoSelected: (!this.previousFilter.estadoSelected) ? 0 : this.previousFilter.estadoSelected,
                habilitacionSelected: (!this.previousFilter.habilitacionSelected) ? 0 : this.previousFilter.habilitacionSelected,
                idInstrumentoSelected: 0
            };
        }

        //this method verify that the item is not annul to allow load de the modal
        annulItem(): void {
            var current = this.instrumentoAccionCollection.currentItem;
            if (current.actividadDescripcion == this.sharedLabels.annulValueText) {
                this.claimMessage.error(this.sharedLabels.disableMessageNOOK);
            }
            else {
                this.validateOpenModal(app.common.eOperation.Annul);
            }
        }

        evaluateConditionValidateOpenModal(operationType: number) {
            var vm = this;
            if (vm.tiposAcciones.length > 0)
                vm.loadModal(operationType);
            else
                vm.claimMessage.error(vm.sharedLabels.groupInstrumentNotExist + app.common.eGrupoInstrumento.Acciones);
        }

        validateOpenModal(operationType: number): void {
            var vm = this;
            if (vm.tiposAcciones.length == 0) {
                vm.tipoInstrumentoService.GetAllByGrupoInstrumento(app.common.eGrupoInstrumento.Acciones, vm.sharedLabels.allText).then(data => {
                    vm.tiposAcciones = data;
                    vm.evaluateConditionValidateOpenModal(operationType);
                },
                    (error: common.IHttpPromiseCallbackErrorArg) => { vm.appSetting.isLoading = false; vm.claimMessage.error(error.data); });
            }
            else {
                vm.loadModal(operationType);
            }
        }

        //This method loads the modal service      
        loadModal(operationType: number, doubleClickDisabled: boolean = false): void {
            var vm = this;

            var isAllowed = vm.verifyPermissions(operationType);
            if (!isAllowed)
                return;

            var instrumentoAccion: app.common.IInstrumentoAccion;
            if (operationType != app.common.eOperation.AddNew) {
                if (vm.instrumentoAccionCollection.currentItem == null) {
                    vm.claimMessage.info(vm.sharedLabels.shoulSelectAnItemToModifiy);
                    return;
                }
                instrumentoAccion = vm.instrumentoAccionCollection.currentItem;
            }
            else {
                instrumentoAccion = <app.common.IInstrumentoAccion>{
                    idInstrumento: -1,
                    idTipoInstrumento: 0,
                    nombreInstrumento: "",
                    codigoSbs: "",
                    codigoSbsGenerated: "",
                    codigoSbsJoined: "",
                    idMoneda: 0,
                    idEmisor: 0,
                    idGrupoInstrumento: null,
                    indCategoria: 0,
                    indFamilia: 0,
                    indActividad: 0,
                    idHabilitacion: 0,
                    indHabilitacionRiesgo: 0,
                    comentarioHabilitacion: "",
                    comentarioAnulacion: "",
                    idClasificacionRiesgo: 0,
                    indClase: 0,
                    idAccion: -1,
                    tieneMonedaDual: false,
                    idMonedaDual: 0,
                    idSecuencialFechaEmision: 0,
                    idSecuencialFechaVencimiento: 0,
                    secuencialFechaEmision: "",
                    secuencialFechaVencimiento: "",
                    valorNominal: "0.0000000",
                    valorNominalSbs: "0.0000000",
                    indTipoCustodia: 0,
                    codIsin: "",
                    nemotecnico: "",
                    montoEmitido: "0.0000000",
                    montoColocado: "0.0000000",
                    idSecuencialFechaMontoEmitido: 0,
                    idSecuencialFechaMontoColocado: 0,
                    secuencialFechaMontoEmitido: "",
                    secuencialFechaMontoColocado: "",
                    indTipoUnidadEmision: 0,
                    nroUnidadesFloat: "0",
                    indPaisEmisor: 0,
                    indFocoGeograficoEmision: 0,
                    indRegionEmision: 0,
                    tieneFechaVencimiento: false,
                    tieneMandato: false,
                    descripcion: "-",
                    fechaEmision: null,
                    fechaMontoColocado: null,
                    fechaMontoEmitido: null,
                    fechaVencimiento: null

                    };
            }
            
            if (instrumentoAccion.isAdrAds) {
                vm.openModalAdrAds(instrumentoAccion, operationType, doubleClickDisabled);
            } else {
                var modalDefaults = {
                    backdrop: true,
                    keyboard: true,
                    modalFade: true,
                    templateUrl: 'app/mesadineroApp/instrumentoAccion/instrumento-accion-detail.html',
                    controller: InstrumentoAccionDetailController,
                    controllerAs: 'vmd',
                    size: "lg",
                    resolve: {
                        instrumentoAccion: instrumentoAccion,
                        operationType: operationType,
                        totalItems: vm.pagination.totalItems,
                        doubleClickDisabled: doubleClickDisabled
                    }
                };

                //If the operation in the modal is succesful
                vm.modalService.showModal(modalDefaults, {}).then((result) => {
                    if (result === 'ok') {
                        if (operationType == app.common.eOperation.Remove)
                            vm.callPreviousPageIfCurrentPageHasOnlyOneItemAndIsRemoved();
                        else
                            vm.getInstrumentoAccionPaged(true);
                    }
                });
            }
        }

        openModalAdrAds(instrumentoAccion: app.common.IInstrumentoAccion, operationType: number, doubleClickDisabled:boolean): void {
            const vm = this;
            /*if (!doubleClickDisabled && operationType == app.common.eOperation.Update) {
                var modalOptions = {
                    cancelButtonText: this.sharedLabels.cancelButtonText,
                    actionButtonText: this.sharedLabels.acceptButtonText,
                    size: 'xs',
                    headerText: this.sharedLabels.alert,
                    bodyText: "No se puede modificar un ADR/ADS directamente, debe ir al instrumento inicial",
                    colorHeader: vm.appSetting.codigoColor
                };
                this.modalService.showModal({}, modalOptions).then((result) => {});
            } else */if (!doubleClickDisabled && (operationType == app.common.eOperation.Annul || operationType == app.common.eOperation.Remove)) {
                var modalDefaults = {
                    backdrop: true,
                    keyboard: true,
                    modalFade: true,
                    templateUrl: 'app/mesadineroApp/instrumentoAccion/definir-adr-ads/instrumento-accion-detail.html',
                    controller: DefinirAdrAdsController,
                    controllerAs: 'vmde',
                    size: "lg",
                    resolve: {
                        operationType: () => { return operationType },
                        doubleClickDisabled: () => { return doubleClickDisabled },
                        currentInstrumentoAccion: () => { return {} },
                        ListadoInstrumentoAccionAdsAds: () => { return [] },
                        isDisabled: () => { return true },
                        IdItem: () => { return { idAccion: instrumentoAccion.idAccion, idInstrumento: instrumentoAccion.idInstrumento } },
                        emisores: () => { return vm.emisores }
                    }
                };

                //If the operation in the modal is succesful
                vm.modalService.showModal(modalDefaults, {}).then((result) => {
                    if (result[0] === 'ok') {
                        /*if (operationType == app.common.eOperation.Remove || operationType == app.common.eOperation.Annul)
                            vm.callPreviousPageIfCurrentPageHasOnlyOneItemAndIsRemoved();
                        else*/
                            vm.getInstrumentoAccionPaged(true);
                    }
                });
            }
            else {
                var modalDefaults1 = {
                    backdrop: true,
                    keyboard: true,
                    modalFade: true,
                    templateUrl: 'app/mesadineroApp/instrumentoAccion/instrumento-accion-detail.html',
                    controller: InstrumentoAccionDetailController,
                    controllerAs: 'vmd',
                    size: "lg",
                    resolve: {
                        instrumentoAccion: instrumentoAccion,
                        operationType: operationType,
                        totalItems: vm.pagination.totalItems,
                        doubleClickDisabled: doubleClickDisabled
                    }
                };

                //If the operation in the modal is succesful
                vm.modalService.showModal(modalDefaults1, {}).then((result) => {
                    if (result === 'ok') {
                        if (operationType == app.common.eOperation.Remove)
                            vm.callPreviousPageIfCurrentPageHasOnlyOneItemAndIsRemoved();
                        else
                            vm.getInstrumentoAccionPaged(true);
                    }
                });

                //var modalDefaults = {
                //    backdrop: true,
                //    keyboard: true,
                //    modalFade: true,
                //    templateUrl: 'app/mesadineroApp/instrumentoAccion/definir-adr-ads/instrumento-accion-detail.html',
                //    controller: DefinirAdrAdsController,
                //    controllerAs: 'vmde',
                //    size: "lg",
                //    resolve: {
                //        operationType: () => { return operationType },
                //        doubleClickDisabled: () => { return doubleClickDisabled },
                //        currentInstrumentoAccion: () => { return {} },
                //        ListadoInstrumentoAccionAdsAds: () => { return [] },
                //        isDisabled: () => { return true },
                //        IdItem: () => { return { idAccion: instrumentoAccion.idAccion, idInstrumento: instrumentoAccion.idInstrumento } },
                //        emisores: () => { return vm.emisores }
                //    }
                //};

                ////If the operation in the modal is succesful
                //vm.modalService.showModal(modalDefaults, {}).then((result) => {
                //    if (result[0] === 'ok') {
                //        if (operationType == app.common.eOperation.Remove || operationType == app.common.eOperation.Annul)
                //            vm.callPreviousPageIfCurrentPageHasOnlyOneItemAndIsRemoved();
                //        else
                //            vm.getInstrumentoAccionPaged(true);
                //    }
                //});
            }
          
        }

        //This method call the previous if in the current page the user has removed the only one item
        callPreviousPageIfCurrentPageHasOnlyOneItemAndIsRemoved(): void {

            if (this.pagination.currentPage == this.pagination.totalPages) {
                if (this.pagination.totalItems == this.pagination.itemsPerPage * (this.pagination.totalPages - 1) + 1)
                    this.pagination.currentPage = (this.pagination.currentPage - 1 > 0) ? this.pagination.currentPage - 1 : 1;
            }
            this.getInstrumentoAccionPaged(false);
        }

        verifyPermissions(operationType: number): boolean {
            var isAllowed = true;
            switch (operationType) {
                case app.common.eOperation.AddNew:
                    if (!this.userPermissions.allowCreate) {
                        this.claimMessage.error(this.sharedLabels.notAllowedToCreate);
                        isAllowed = false;
                    }
                    break;
                case app.common.eOperation.Update:
                    if (!this.userPermissions.allowUpdate) {
                        this.claimMessage.error(this.sharedLabels.notAllowedToUpdate);
                        isAllowed = false;
                    }
                    break;
                case app.common.eOperation.Annul:
                    if (!this.userPermissions.allowAnnul) {
                        this.claimMessage.error(this.sharedLabels.notAllowedToAnnul);
                        isAllowed = false;
                    }
                    break;
                case app.common.eOperation.Remove:
                    if (!this.userPermissions.allowRemove) {
                        this.claimMessage.error(this.sharedLabels.notAllowedToRemove);
                        isAllowed = false;
                    }
                    break;
            }
            return isAllowed;
        }

        //This method is the controller because the html needs to call it
        cleanStringOfInvalidCharacters(fullText: string): string {
            return app.common.cleanStringOfInvalidCharacters(fullText);
        }
    }

    angular
        .module('app.mesadineroapp.instrumentoAccion')
        .controller(InstrumentoAccionListController.controllerId, InstrumentoAccionListController);
}  