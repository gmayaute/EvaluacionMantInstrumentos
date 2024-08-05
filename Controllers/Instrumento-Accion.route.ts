interface IInstrumentoAccionParams extends ng.route.IRouteParamsService {
    idInstrumentoAccion: number;
}

((): void => {
    'use strict';

    angular
        .module('app.mesadineroapp.instrumentoAccion')
        .config(config);

    config.$inject = [
        '$routeProvider',
        '$locationProvider'
    ];
    function config(
        $routeProvider: ng.route.IRouteProvider,
        $locationProvider: ng.ILocationProvider): void {
        $routeProvider
            .when('/instrumento-accion', {
                templateUrl: 'app/mesadineroApp/instrumentoAccion/instrumento-accion-list.html',
                controller: 'app.mesadineroapp.instrumentoAccion.InstrumentoAccionListController',
                controllerAs: 'vm'
            });
    }
})();

