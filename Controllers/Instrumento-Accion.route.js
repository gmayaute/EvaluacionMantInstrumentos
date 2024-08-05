(function () {
    'use strict';
    angular
        .module('app.mesadineroapp.instrumentoAccion')
        .config(config);
    config.$inject = [
        '$routeProvider',
        '$locationProvider'
    ];
    function config($routeProvider, $locationProvider) {
        $routeProvider
            .when('/instrumento-accion', {
            templateUrl: 'app/mesadineroApp/instrumentoAccion/instrumento-accion-list.html',
            controller: 'app.mesadineroapp.instrumentoAccion.InstrumentoAccionListController',
            controllerAs: 'vm'
        });
    }
})();
//# sourceMappingURL=instrumento-accion.route.js.map