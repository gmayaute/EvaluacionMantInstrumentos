USE [FMS_PI]
GO

/****** Object:  View [FMS].[VW_ListadoInstrumentos]    Script Date: 23/12/2021 11:10:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [FMS].[VW_ListadoInstrumentos]
AS
	WITH Instrumentos
	AS (
		SELECT  IdAccion AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				CodIsin AS CodigoIsin,
				IndPaisEmisor AS IndPaisEmision,
				ValorNominal,
				ValorNominalSbs,
				NroUnidadesFloat,
				IdSecuencialFechaVencimiento AS IdFechaVencimiento,
				IdSecuencialFechaEmision AS IdFechaEmision,
				MontoEmitido AS ValorContable,
				MontoColocado AS ValorDisponible,
				IndClase,
				EsPadre = IIF(ISNULL(IsAdrAds, 0) = 0, 1, 0),
				IndTipoAccionSbs AS IndTipoAccionSbs,
				ValorNominal AS ValorNominalInicial
		FROM FMS.Accion WITH (NOLOCK)
		UNION ALL
		SELECT  IdRentaFija AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				CodigoIsin,
				IndPaisEmision,
				ValorNominalVigente AS ValorNominal,
				ValorNominalSbs,
				NULL AS NroUnidadesFloat,
				IdFechaVencimiento,
				IdFechaEmision,
				MontoEmitido AS ValorContable,
				MontoColocado AS ValorDisponible,
				IndClase,
				EsPadre = IIF(ISNULL(IsGdn, 0) = 0, 1, 0),
				NULL AS IndTipoAccionSbs,
				ValorNominalInicial AS ValorNominalInicial
		FROM FMS.RentaFija WITH (NOLOCK)
		UNION ALL
		SELECT  IdCertificadoSuscripcionPreferente AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				CodigoIsin,
				0 AS IndPaisEmision,
				ValorNominalInicial AS ValorNominal,
				ValorNominalSbs,
				NULL AS NroUnidadesFloat,
				IdSecuencialFechaFinNegociacion AS IdFechaVencimiento,
				IdSecuencialFechaMontoEmitido AS IdFechaEmision,
				MontoEmitido AS ValorContable,
				MontoColocado AS ValorDisponible,
				null AS IndClase,
				NULL AS EsPadre,
				NULL AS IndTipoAccionSbs,
				ValorNominalInicial AS ValorNominalInicial
		FROM FMS.CertificadoSuscripcionPreferente WITH (NOLOCK)
		UNION ALL
		SELECT  IdFondoMutuo AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				CodigoIsin,
				IndPaisEmision,
				ValorNominalInicial AS ValorNominal,
				ValorNominalSbs,
				NULL AS NroUnidadesFloat,
				IdSecuencialFechaVencimiento AS IdFechaVencimiento,
				IdSecuencialFechaInicio AS IdFechaEmision,
				MontoEmitido AS ValorContable,
				MontoColocado AS ValorDisponible,
				IndClase,
				NULL AS EsPadre,
				NULL AS IndTipoAccionSbs,
				ValorNominalInicial AS ValorNominalInicial
		FROM FMS.FondoMutuo WITH (NOLOCK)
		UNION ALL
		SELECT  IdFondoAlternativo AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				CodigoIsin,
				IndPaisEmision,
				ValorNominalInicial AS ValorNominal,
				ValorNominalSbs,
				NULL AS NroUnidadesFloat,
				IdSecuencialFechaCierre AS IdFechaVencimiento,
				IdSecuencialFechaInicio AS IdFechaEmision,
				MontoEmitido AS ValorContable,
				MontoColocado AS ValorDisponible,
				IndClase,
				NULL AS EsPadre,
				NULL AS IndTipoAccionSbs,
				ValorNominalInicial AS ValorNominalInicial
		FROM FMS.FondoAlternativo WITH (NOLOCK)		
		UNION ALL
		SELECT  IdFuturo AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				'' AS CodigoIsin,
				0 AS IndPaisEmision,
				0 AS ValorNominal,
				0 AS ValorNominalSbs,
				NULL AS NroUnidadesFloat,
				0 AS IdFechaVencimiento,
				IdSecuencialFechaEmision AS IdFechaEmision,
				0 AS ValorContable,
				0 AS ValorDisponible,
				0 AS IndClase,
				NULL AS EsPadre,
				NULL AS IndTipoAccionSbs,
				0 AS ValorNominalInicial
		FROM FMS.Futuro WITH (NOLOCK)
		UNION ALL
		SELECT  IdOpcion AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				'' AS CodigoIsin,
				0 AS IndPaisEmision,
				0 AS ValorNominal,
				0 AS ValorNominalSbs,
				NULL AS NroUnidadesFloat,
				0 AS IdFechaVencimiento,
				IdSecuencialFechaEmision AS IdFechaEmision,
				0 AS ValorContable,
				0 AS ValorDisponible,
				0 AS IndClase,
				NULL AS EsPadre,
				NULL AS IndTipoAccionSbs,
				0 AS ValorNominalInicial
		FROM FMS.Opcion WITH (NOLOCK)
		UNION ALL
		SELECT  IdCertificadoDepositoCortoPlazo AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				CodigoIsin,
				IndPaisEmision,
				ValorNominalVigente,
				ValorNominalSbs,
				NULL AS NroUnidadesFloat,
				IdSecuencialFechaVencimiento AS IdFechaVencimiento,
				IdSecuencialFechaEmision AS IdFechaEmision,
				MontoEmitido AS ValorContable,
				MontoColocado AS ValorDisponible,
				IndClase,
				NULL AS EsPadre,
				NULL AS IndTipoAccionSbs,
				ValorNominal AS ValorNominalInicial
		FROM FMS.CertificadoDepositoCortoPlazo WITH (NOLOCK)
		UNION ALL
		SELECT  IdNotaEstructurada AS IdInstrumentoHijo,
				IdInstrumento,
				Nemotecnico,
				TieneMandato,
				CodigoIsin,
				0 AS IndPaisEmision,
				ValorNominalInicial AS ValorNominal,
				ValorNominalSbs,
				NULL AS NroUnidadesFloat,
				IdSecuencialFechaVencimiento AS IdFechaVencimiento,
				IdSecuencialFechaInicio AS IdFechaEmision,
				0 AS ValorContable,
				MontoColocado AS ValorDisponible,
				IndClase,
				NULL AS EsPadre,
				NULL AS IndTipoAccionSbs,
				ValorNominalInicial AS ValorNominalInicial
		FROM FMS.NotaEstructurada WITH (NOLOCK)
	)
	SELECT  i.IdInstrumento,
			i.IdInstrumentoHijo,
			ins.CodigoSbs,
			ins.NombreInstrumento,
			ins.IdMoneda,
			ins.IdTipoInstrumento,
			ins.IdGrupoInstrumento,
			i.Nemotecnico,
			i.TieneMandato,
			ins.IndActividad,
			ins.IndHabilitacionRiesgo,
			i.CodigoIsin,
			i.IndPaisEmision,
			i.ValorNominal,
			i.ValorNominalSbs,
			i.NroUnidadesFloat,
			i.IdFechaVencimiento,
			ins.IdEmisor,
			i.IdFechaEmision,
			i.ValorContable,
			i.ValorDisponible,
			i.IndClase,
			i.EsPadre,
			i.IndTipoAccionSbs,
			ins.IdClasificacionRiesgo,
			i.ValorNominalInicial,
			ins.factornominal
	FROM Instrumentos AS i
	INNER JOIN FMS.Instrumento AS ins WITH (NOLOCK) ON i.IdInstrumento = ins.IdInstrumento

GO


