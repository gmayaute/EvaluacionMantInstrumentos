
module app.services {
    'use strict';

    export interface IInstrumentoAccionService {
        getByIdInstrumentoAccion(idInstrumento: number, idAccion: number): ng.IPromise<app.common.IInstrumentoAccion>;
        getFilteredData(pagination: app.common.IPaged, instrumentoAccion: app.common.IInstrumentoAccionFilter): ng.IPromise<app.common.IInstrumentoAccionPaged>;
        update(instrumentoAccion: app.common.IInstrumentoAccion): ng.IPromise<string>;
        create(instrumentoAccion: app.common.IInstrumentoAccion): ng.IPromise<string>;
        remove(idInstrumento: number, idAccion: number): ng.IPromise<string>;
        disable(instrumentoAccion: app.common.IInstrumentoAccion): ng.IPromise<string>;
        getAllAccionByActiveRiesgo(grupo: any, firstTextItem?: string): ng.IPromise<app.common.IInstrumentoAccion[]>;
        activeInstrumento(accion: app.common.IInstrumentoAccion): ng.IPromise<string>;
        getAllInstrumentosAcciones(parametros: app.common.IBusquedaInstrumento): ng.IPromise<app.common.IInstrumentoAccion[]>;
        getAll(): ng.IPromise<app.common.IInstrumentoAccion[]>;

        getAllInstrumentosAccionLiberadaAjuste(acuerdoBusqueda: app.common.IBusquedaInstrumentoAcuerdoFilter): ng.IPromise<app.common.IBusquedaInstrumentoAcuerdos[]>;
    }

    class InstrumentoAccionService implements IInstrumentoAccionService {
        public static controllerId: string = 'app.services.InstrumentoAccionService';

        constructor(private $http: ng.IHttpService,
            private apiEndpoint: app.blocks.IApiEndpointConfig) { }

        getByIdInstrumentoAccion(idInstrumento: number, idAccion: number): ng.IPromise<app.common.IInstrumentoAccion> {
            return this.$http
                .get(this.apiEndpoint.baseUrl + '/instrumentoAccion/GetByIdInstrumentoAccion/' + idInstrumento + '/' + idAccion)
                .then((response: ng.IHttpPromiseCallbackArg<app.common.IInstrumentoAccion>): app.common.IInstrumentoAccion => {
                    return <app.common.IInstrumentoAccion>response.data;
                });
        }


        getFilteredData(pagination: app.common.IPaged, instrumentoAccion: app.common.IInstrumentoAccionFilter): ng.IPromise<app.common.IInstrumentoAccionPaged> {
            pagination.currentPage = (pagination.currentPage == null) ? 1 : pagination.currentPage;
            return this.$http
                .get(this.apiEndpoint.baseUrl + '/instrumentoAccion/GetFilteredDataAcciones/' + instrumentoAccion.codigoSbs + '/' + instrumentoAccion.codigoIsin + '/' + instrumentoAccion.tipoAccionSelected + '/' + instrumentoAccion.emisorSelected + '/' + instrumentoAccion.monedaSelected + '/' + instrumentoAccion.estadoSelected + '/' + instrumentoAccion.habilitacionSelected + '/' + instrumentoAccion.idInstrumentoSelected + '/' + pagination.currentPage + '/' + pagination.itemsPerPage + '/' + pagination.orderColumn + '/' + pagination.isAscending)
                .then((response: ng.IHttpPromiseCallbackArg<app.common.IInstrumentoAccionPaged>): app.common.IInstrumentoAccionPaged => {
                    return <app.common.IInstrumentoAccionPaged>response.data;
                });
        }

        update(instrumentoAccion: app.common.IInstrumentoAccion): ng.IPromise<string> {
            return this.$http
                .put(this.apiEndpoint.baseUrl + '/instrumentoAccion/UpdateInstrumentoAccion', instrumentoAccion)
                .then((response: ng.IHttpPromiseCallbackArg<string>): string => {
                    return <string>response.data;
                });
        }

        create(instrumentoAccion: app.common.IInstrumentoAccion): ng.IPromise<string> {
            return this.$http
                .post(this.apiEndpoint.baseUrl + '/instrumentoAccion/AddNewInstrumentoAccion', instrumentoAccion)
                .then((response: ng.IHttpPromiseCallbackArg<string>): string => {
                    return <string>response.data;
                });
        }

        remove(idInstrumento: number, idAccion: number): ng.IPromise<string> {
            return this.$http
                .delete(this.apiEndpoint.baseUrl + '/instrumentoAccion/RemoveInstrumentoAccion/' + idInstrumento + '/' + idAccion)
                .then((response: ng.IHttpPromiseCallbackArg<string>): string => {
                    return <string>response.data;
                });
        }

        disable(instrumentoAccion: app.common.IInstrumentoAccion): ng.IPromise<string> {
            return this.$http
                .put(this.apiEndpoint.baseUrl + '/instrumentoAccion/AnnulInstrumentoAccion', instrumentoAccion)
                .then((response: ng.IHttpPromiseCallbackArg<string>): string => {
                    return <string>response.data;
                });
        }

        getAllAccionByActiveRiesgo(grupo: any, firstTextItem: string = null): ng.IPromise<app.common.IInstrumentoAccion[]> {
            return this.$http
                .get(this.apiEndpoint.baseUrl + '/instrumentoAccion/GetAllAccionByActiveRiesgo/' + grupo)
                .then((response: ng.IHttpPromiseCallbackArg<app.common.IInstrumentoAccion[]>): app.common.IInstrumentoAccion[] => {
                    if (firstTextItem != null)
                        response.data.unshift(<app.common.IInstrumentoAccion>{ idAccion: 0, nombreInstrumento: firstTextItem, codigoSbs: firstTextItem, idInstrumento: 0 });
                    return <app.common.IInstrumentoAccion[]>response.data;
                });
        }

        activeInstrumento(accion: app.common.IInstrumentoAccion): ng.IPromise<string> {
            return this.$http
                .put(this.apiEndpoint.baseUrl + '/instrumentoAccion/ActiveInstrumentoAccion/', accion)
                .then((response: ng.IHttpPromiseCallbackArg<string>): string => {
                    return <string>response.data;
                });
        }

        getAllInstrumentosAcciones(parametros: app.common.IBusquedaInstrumento): ng.IPromise<app.common.IInstrumentoAccion[]> {
            return this.$http
                .get(this.apiEndpoint.baseUrl + '/instrumentoAccion/GetAllInstrumentosAcciones/' + parametros.idFondoDestino + '/' + parametros.idInstrumento + '/' +
                    parametros.codigoIsin + '/' + parametros.idEmisor + '/' + parametros.codigoSbs + '/' + parametros.nemotecnico)
                .then((response: ng.IHttpPromiseCallbackArg<app.common.IInstrumentoAccion[]>): app.common.IInstrumentoAccion[] => {
                    return <app.common.IInstrumentoAccion[]>response.data;
                });
        }

        getAll(): ng.IPromise<app.common.IInstrumentoAccion[]> {
            return this.$http
                .get(this.apiEndpoint.baseUrl + '/instrumentoAccion/GetAll')
                .then((response: ng.IHttpPromiseCallbackArg<app.common.IInstrumentoAccion[]>): app.common.IInstrumentoAccion[] => {
                    return <app.common.IInstrumentoAccion[]>response.data;
                });
        }

        getAllInstrumentosAccionLiberadaAjuste(acuerdoBusqueda: app.common.IBusquedaInstrumentoAcuerdoFilter): ng.IPromise<app.common.IBusquedaInstrumentoAcuerdos[]> {
            return this.$http
                .get(this.apiEndpoint.baseUrl + '/instrumentoAccion/GetAllInstrumentosAccionLiberadaAjuste', ({
                    params: acuerdoBusqueda
                }))
                .then((response: ng.IHttpPromiseCallbackArg<app.common.IBusquedaInstrumentoAcuerdos[]>): app.common.IBusquedaInstrumentoAcuerdos[] => {
                    return <app.common.IBusquedaInstrumentoAcuerdos[]>response.data;
                });
        }

    }


    factory.$inject = [
        '$http',
        'app.blocks.ApiEndpoint'
    ];
    function factory($http: ng.IHttpService,
        apiEndpoint: app.blocks.IApiEndpointConfig): IInstrumentoAccionService {
        return new InstrumentoAccionService($http, apiEndpoint);
    }

    angular
        .module('app.services')
        .factory(InstrumentoAccionService.controllerId, factory);
}