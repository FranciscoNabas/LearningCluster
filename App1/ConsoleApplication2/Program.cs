using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        public static void Main(string[] args)
        {
            string TempoLaranja = "7";
            string TempoAmarelo = "8";
            string TempoVerde = "9";
            string TempoAzul = "10";
            string ConnectionString = "Data Source=10.0.0.95;Initial Catalog=HospitalarBarueriQualidade;User ID=adm_saude;Password=adm_saude;";
            string SqlCmd = $@"ALTER PROC DBO.P045_PACIENTESTEMPOEXCEDIDOS
                            @REC INT=NULL,
                            @CONTADOR INT=NULL,
                            @ID_PRI INT=NULL,
                            @PRO VARCHAR(255)=NULL,
                            @OP TINYINT=NULL,
                            @ESPSELECIONADA VARCHAR(255)=NULL,
                            @MPS INT=NULL

                            AS
                            SET NOCOUNT ON
                            SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
                            
                            DECLARE @IDREC INT, @ESUP INT
                            DECLARE @DRF DATETIME
                            DECLARE @RECORIGINAL INT
                            
                            DECLARE @EORGTRIAGEM TABLE (ITEM INT)
                            DECLARE @EORGACOLHIMENTO TABLE (ITEM INT)

                            IF @REC IS NOT NULL
                            BEGIN
                            
                            SET @IDREC=(SELECT ID FROM EORG WHERE EORG=@REC)
                            SET @ESUP=(SELECT ESUP FROM EORG WHERE EORG=@REC)
                            
                            IF @IDREC=102
                            BEGIN
                            SELECT @REC=ESUP,@RECORIGINAL=EORG FROM EORG WHERE EORG=@REC
                            END
                            
                            INSERT INTO @EORGTRIAGEM (ITEM)
                            SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@REC AND ID=101
                            
                            INSERT INTO @EORGACOLHIMENTO (ITEM)
                            SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@REC AND ID=100
                            --===========================================SELECIONA PACIENTES NA TELA DO MEDICO================================================
                            
                            IF EXISTS(SELECT 1 FROM EORG E,EORG ES1,EORG ES2 WHERE E.ESUP=ES1.EORG AND ES1.ESUP=ES2.EORG AND E.EORG=@REC AND (E.ID=4 OR ES1.ID=4 OR ES2.ID=4))
                            BEGIN
                            SET @DRF=DATEADD(HOUR,-72,GETDATE())
                            END
                            ELSE IF EXISTS(SELECT 1 FROM EORG E,EORG ES1,EORG ES2 WHERE E.ESUP=ES1.EORG AND ES1.ESUP=ES2.EORG AND E.EORG=@REC AND (E.ID=3 OR ES1.ID=3 OR ES2.ID=3))
                            BEGIN
                            SET @DRF=DATEADD(HOUR,-24,GETDATE())
                            END
                            ELSE
                            BEGIN
                            SET @DRF=DATEADD(YEAR,-10,GETDATE())
                            END


                            DECLARE @AM TABLE(PAC INT, ID INT)
                            DECLARE @VE TABLE(PAC INT, ID INT)
                            DECLARE @AZ TABLE(PAC INT, ID INT)
                            DECLARE @VER TABLE(PAC INT, ID INT)
                            DECLARE @LA TABLE(PAC INT, ID INT)
                            
                            INSERT INTO @AM(PAC, ID)
                            SELECT PP.PAC, 3
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL) 
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=3
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoAmarelo}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            INSERT INTO @VE(PAC, ID)
                            SELECT PP.Pac, 4
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=4
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoVerde}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            INSERT INTO @AZ(PAC, ID)
                            SELECT PP.PAC, 5
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=5
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoAzul}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            INSERT INTO @VER(PAC, ID)
                            SELECT PP.Pac,1
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=1
                            AND DATEDIFF(HOUR,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>=0
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            INSERT INTO @LA(PAC, ID)
                            SELECT PP.PAC,2
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=2
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoLaranja}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            DECLARE @TPAC  TABLE (PAC INT,NPF INT,NOME VARCHAR(255),DNS DATETIME,DAT DATETIME,DESP VARCHAR(255),NMPS VARCHAR(255),SEQ INT,DPA VARCHAR(255),
                            DSX VARCHAR(255),TIPOATEND VARCHAR(255),ESPERA VARCHAR(255),ID_PRI TINYINT,MPS INT,NPFMPS INT,ESP INT,SENHA INT, PONTUACAO INT,TEMPO INT,SALA INT,DAM DATETIME, 
                            SQA INT, APL INT, DTC DATETIME, DESCONHECIDO VARCHAR(255), ID_PESSOANEXT INT)
                            
                            DECLARE  @TP TABLE (PAC INT,ID_PRI TINYINT,PONTUACAO INT, SENHA INT, SQA INT,DTC DATETIME)
                            														
                            --==========CARREGA PACIENTES QUANDO SETOR TEM TRIAGEM E ACOLHIMENTO=====================================
                            IF EXISTS (SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@REC AND ID=101) AND EXISTS (SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@REC AND ID=100)
                            BEGIN
                            
                            INSERT INTO @TP(PAC,ID_PRI,PONTUACAO,SENHA,SQA,DTC)
                            SELECT P.PAC,PP.ID_PRI, PP.PONTUACAO, ISNULL(PP.NRT,P.SEQ) SENHA, P.SQA,P.DT_CH
                            FROM PACIENTES P,PESSOA_FISICA F,PACIENTES_TRIAGEM PP
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL)
                            AND ISNULL(P.LOC_I,P.LOC)=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.APL=257
                            AND PP.PAC=P.PAC AND PP.EORG IN(SELECT ITEM FROM @EORGTRIAGEM) AND PP.DT_FINAL IS NOT NULL AND P.DAT>@DRF AND PP.ID_PRI=@ID_PRI 
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            
                            INSERT INTO @TP(PAC,ID_PRI,PONTUACAO,SENHA,SQA,DTC)
                            SELECT P.PAC,PP.ID_PRI,PP.PONTUACAO,ISNULL(PP.NRT,P.SEQ) SENHA, P.SQA,PP.Dtc
                            FROM PACIENTES P,PESSOA_FISICA F,PACIENTES_TRIAGEM PP
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL)
                            AND ISNULL(P.LOC_I,P.LOC)=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.APL=257
                            AND PP.PAC=P.PAC AND PP.EORG IN(SELECT ITEM FROM @EORGACOLHIMENTO) AND PP.DT_FINAL IS NOT NULL AND P.DAT>@DRF AND PP.ID_PRI=@ID_PRI 
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            AND NOT EXISTS(SELECT 1 FROM @TP Y WHERE Y.PAC=P.PAC)
                            
                            INSERT INTO @TP(PAC,ID_PRI,SENHA,SQA,DTC)
                            SELECT P.PAC,1,P.SEQ,P.SQA,P.DT_CH
                            FROM PACIENTES P 
                            LEFT JOIN PACIENTES_TRIAGEM PP ON PP.PAC=P.PAC,PESSOA_FISICA F
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL) AND ISNULL(P.LOC_I,P.LOC)=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.APL=257 AND P.DAT>@DRF
                            AND DBO.F045_VERIFICATRIAGEMESPTIPA (ISNULL(P.LOC_I,P.LOC),P.ESP,P.TIPA)=0 AND PP.ID_PRI=@ID_PRI 
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            AND NOT EXISTS(SELECT 1 FROM @TP Y WHERE Y.PAC=P.PAC)
                            PRINT 'CARREGA PACIENTES QUANDO SETOR TEM TRIAGEM E ACOLHIMENTO'
                            END
                            
                            --================CARREGA PACIENTES QUANDO SETOR TEM TRIAGEM =====================================
                            ELSE IF EXISTS (SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@REC AND ID=101) AND NOT EXISTS (SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@REC AND ID=100)
                            BEGIN
                            INSERT INTO @TP(PAC,ID_PRI,PONTUACAO,SENHA,SQA,DTC)
                            SELECT P.PAC,PP.ID_PRI,PP.PONTUACAO,ISNULL(PP.NRT,P.SEQ) SENHA,P.SQA,PP.Dtc
                            FROM PACIENTES P,PESSOA_FISICA F,PACIENTES_TRIAGEM PP
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL)
                            AND ISNULL(P.LOC_I,P.LOC)=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.APL=257
                            AND PP.PAC=P.PAC AND PP.EORG IN(SELECT ITEM FROM @EORGTRIAGEM) AND PP.DT_FINAL IS NOT NULL AND P.DAT>@DRF AND PP.ID_PRI=@ID_PRI 
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            
                            
                            INSERT INTO @TP(PAC,ID_PRI,SENHA,SQA,DTC)
                            SELECT P.PAC,CASE WHEN P.TIPA IN(16,19) THEN 1 ELSE 0 END,P.SEQ,P.SQA,P.DT_CH DTC
                            FROM PACIENTES P LEFT JOIN PACIENTES_TRIAGEM PP ON PP.PAC=P.PAC,PESSOA_FISICA F
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL) 
                            AND ISNULL(P.LOC_I,P.LOC)=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.APL=257 AND P.DAT>@DRF
                            AND DBO.F045_VERIFICATRIAGEMESPTIPA (ISNULL(P.LOC_I,P.LOC),P.ESP,P.TIPA)=0 AND PP.ID_PRI=@ID_PRI 
                            AND NOT EXISTS(SELECT 1 FROM @TP Y WHERE Y.PAC=P.PAC)
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            PRINT 'CARREGA PACIENTES QUANDO SETOR TEM TRIAGEM'
                            END
                            
                            
                            --==============CARREGA PACIENTES QUANDO SETOR TEM ACOLHIMENTO =====================================
                            ELSE IF NOT EXISTS (SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@REC AND ID=101) AND EXISTS (SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@REC AND ID=100)
                            BEGIN
                            INSERT INTO @TP(PAC,ID_PRI,PONTUACAO,SENHA,SQA,DTC)
                            SELECT P.PAC,PP.ID_PRI,PP.PONTUACAO,ISNULL(PP.NRT,P.SEQ) SENHA, P.SQA,PP.Dtc
                            FROM PACIENTES P,PESSOA_FISICA F,PACIENTES_TRIAGEM PP
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL)
                            AND ISNULL(P.LOC_I,P.LOC)=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.APL=257
                            AND PP.PAC=P.PAC AND PP.EORG IN(SELECT ITEM FROM @EORGACOLHIMENTO) AND PP.DT_FINAL IS NOT NULL AND P.DAT>@DRF AND PP.ID_PRI=@ID_PRI 
                            AND 
                            (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            AND NOT EXISTS(SELECT 1 FROM @TP Y WHERE Y.PAC=P.PAC)
                            
                            INSERT INTO @TP(PAC,ID_PRI,SENHA,SQA,DTC)
                            SELECT P.PAC,0,P.SEQ,P.SQA,P.DT_CH DTC
                            FROM PACIENTES P 
                            LEFT JOIN PACIENTES_TRIAGEM PP ON PP.PAC=P.PAC,PESSOA_FISICA F
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL) AND ISNULL(P.LOC_I,P.LOC)=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.APL=257 
                            AND P.DAT>@DRF
                            AND DBO.F045_VERIFICATRIAGEMESPTIPA (ISNULL(P.LOC_I,P.LOC),P.ESP,P.TIPA)=0 AND PP.ID_PRI=@ID_PRI 
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            AND NOT EXISTS(SELECT 1 FROM @TP Y WHERE Y.PAC=P.PAC)
                            
                            PRINT 'CARREGA PACIENTES QUANDO SETOR TEM ACOLHIMENTO'
                            END
                            ELSE--==============CARREGA PACIENTES NÃO TEM TRIAGEM E NEM ACOLHIMENTO =====================================
                            BEGIN
                            INSERT INTO @TP (PAC,SENHA,SQA,DTC)
                            SELECT P.PAC,P.SEQ,P.SQA,P.DT_CH DTC 
                            FROM PACIENTES P 
                            LEFT JOIN PACIENTES_TRIAGEM PP ON PP.PAC=P.PAC,PESSOA_FISICA F
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL)
                            AND P.LOC_I=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.DAT>@DRF AND PP.ID_PRI=@ID_PRI 
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            UNION
                            SELECT P.PAC,P.SEQ,P.SQA,P.DT_CH DTC 
                            FROM PACIENTES P 
                            LEFT JOIN PACIENTES_TRIAGEM PP ON PP.PAC=P.PAC,PESSOA_FISICA F
                            WHERE P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL)
                            AND P.LOC=@REC AND P.LOC_I IS NULL AND F.NOME LIKE '%'+@PRO+'%' AND P.DAT>@DRF AND PP.ID_PRI=@ID_PRI 
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            UNION
                            SELECT P.PAC,P.SEQ,P.SQA,P.DT_CH DTC 
                            FROM PACIENTES P 
                            LEFT JOIN PACIENTES_TRIAGEM PP ON PP.PAC=P.PAC,EORG LT,EORG QT,EORG UN,PESSOA_FISICA F
                            WHERE LT.EORG=P.LH AND QT.EORG=LT.ESUP AND UN.EORG=QT.ESUP AND P.NPF=F.NPF AND P.DT_CANC IS NULL AND (P.DAM IS NULL OR P.DST IS NULL)
                            AND UN.EORG=@REC AND F.NOME LIKE '%'+@PRO+'%' AND P.DAT>@DRF AND PP.ID_PRI=@ID_PRI 
                            AND (EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.Pac AND I.ID = @ID_PRI))
                            PRINT 'CARREGA PACIENTES NÃO TEM TRIAGEM E NEM ACOLHIMENTO'
                            END
                            
                            INSERT INTO @TPAC (PAC ,NPF ,NOME ,DNS ,DAT ,DESP ,NMPS ,SEQ ,DPA ,DSX ,TIPOATEND ,ESPERA ,ID_PRI,MPS,NPFMPS,ESP,SENHA,PONTUACAO,TEMPO,DAM,SQA,APL,DTC, DESCONHECIDO, ID_PESSOANEXT)
                            SELECT P.PAC,F.NPF,UPPER(CASE WHEN P.NPF = (SELECT TOP 1 NPF_GENERICO FROM CONFIGURACAO_01) THEN ISNULL(UPPER(P.NOMEDESCONHECIDO),(SELECT PP.NOMEDESCONHECIDO FROM PACIENTES PP WHERE PP.PAC = P.NATEND_INT)) ELSE F.NOME END) NOME
                            ,F.DNS,P.DAT,S.DAB DESP,M.NOME NMPS,P.SEQ,P.DPA,X.DESCRICAO DSX, PT.DESCRICAO TIPOATEND, 
                            CASE WHEN @IDREC IN (2,7,8) THEN CONVERT(VARCHAR(255),DATEDIFF(DAY,PP.DT_FINAL,GETDATE())) + ' DIAS' 
                            ELSE CASE WHEN DATEDIFF(MINUTE,PP.DT_FINAL,GETDATE()) > 60 THEN CASE WHEN LEFT(CONVERT(VARCHAR(255),DATEADD(MINUTE,DATEDIFF(MINUTE,PP.DT_FINAL,GETDATE()),0),108),2) = '00' THEN '24' 
                            ELSE LEFT(CONVERT(VARCHAR(255),DATEADD(MINUTE,DATEDIFF(MINUTE,PP.DT_FINAL,GETDATE()),0),108),2) END + ' HORAS' 
                            ELSE CONVERT(VARCHAR(255),DATEDIFF(MINUTE,PP.DT_FINAL,GETDATE())) + ' MINUTOS' END END ESPERA,Y.ID_PRI,P.MPS,C.NPF,P.ESP,Y.SENHA,Y.PONTUACAO,
                            DATEDIFF(MINUTE,PP.DT_FINAL,GETDATE()),P.DAM,P.SQA,P.APL,Y.DTC, ISNULL(P.IDADEDESCONHECIDO, ''), F.ID_PESSOANEXT
                            FROM @TP Y,PACIENTES P 
                            LEFT JOIN PACIENTES_TRIAGEM PP ON PP.PAC=P.PAC
                            LEFT JOIN CORPOCLINICO C ON C.MPS=P.MPS 
                            LEFT JOIN PESSOA_FISICA M ON M.NPF=C.NPF 
                            LEFT JOIN PACIENTES_TIPA PT ON PT.TIPA=P.TIPA,PESSOA_FISICA F 
                            LEFT JOIN PRONTUARIOS PR ON PR.NPF=F.NPF,SEXO X,PRODUTOS_SERVICOS S
                            WHERE  Y.PAC=P.PAC AND P.NPF=F.NPF AND F.SX=X.SX AND P.ESP=S.COD AND P.DT_CANC IS NULL AND P.DAM IS NULL AND P.DST IS NULL AND P.TipA <> 24
                            AND P.IAC IS NULL 
                            
                            UPDATE @TPAC SET SALA=CAST(DBO.F001_VALORNUMERICO(SL.SGL) AS INT)
                            FROM @TPAC Y,CORPOCLINICO C,EORG_PROFISSIONAIS_SALAS PS,EORG SL
                            WHERE Y.NPFMPS=C.NPF AND C.MPS=PS.MPS AND C.ST=1 AND PS.ST=1 AND PS.SALA=SL.EORG AND SL.ESUP=ISNULL(@RECORIGINAL,@REC)
                            
                            
                            IF	@OP = 0
                            BEGIN
                            SELECT TOP 501 PAC,NPF,NOME,DNS,DAT,DESP,NMPS,SEQ,DPA,DSX,TIPOATEND,ESPERA,ID_PRI,MPS,SENHA,ISNULL(PONTUACAO,37) PONTUACAO,TEMPO,SALA,DTC,DESCONHECIDO,Y.ID_PESSOANEXT,
                            ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = Y.PAC AND E.ESUP = @REC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 GROUP BY PC.PAC),0) QCH,DAM,NPFMPS,SQA,APL
                            FROM @TPAC Y,DBO.F001_SPLIT(@ESPSELECIONADA,'|') ES
                            WHERE ES.ITEM=Y.ESP AND Y.MPS=@MPS AND Y.TEMPO <= 1440
                            ORDER BY PONTUACAO DESC,TEMPO DESC
                            RETURN
                            END
                            ELSE IF	@OP = 1
                            BEGIN
                            SELECT TOP 501 PAC,NPF,NOME,DNS,DAT,DESP,NMPS,SEQ,DPA,DSX,TIPOATEND,ESPERA,ISNULL(ID_PRI,99) ID_PRI,MPS,SENHA,ISNULL(PONTUACAO,37) PONTUACAO,TEMPO,SALA,DTC, DESCONHECIDO,
                            Y.ID_PESSOANEXT,ISNULL((SELECT COUNT(*) 
                            FROM PAINEL_ELETRONICO_CHAMADAS PC 
                            INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS 
                            INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = Y.PAC AND E.ESUP = @REC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 GROUP BY PC.PAC),0) QCH,DAM,NPFMPS,SQA,APL
                            FROM @TPAC Y,DBO.F001_SPLIT(@ESPSELECIONADA,'|') ES
                            WHERE Y.TEMPO <= 1440 AND ES.ITEM=Y.ESP and NMPS is null AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = Y.PAC AND E.ESUP = @REC AND SP.ST=1 AND ISNULL(PC.ST,1)=1  GROUP BY PC.PAC),0) < (SELECT VALOR FROM Eorg_Parametros WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@REC)
                            ORDER BY PONTUACAO DESC,TEMPO DESC
                            RETURN
                            END	
                            ELSE IF	@OP = 2
                            BEGIN
                            SELECT TOP 501 Y.PAC,Y.NPF,Y.NOME,Y.DNS,Y.DAT,Y.DESP,Y.NMPS,Y.SEQ,Y.DPA,Y.DSX,Y.TIPOATEND,Y.ESPERA,Y.ID_PRI,Y.MPS,Y.SENHA,ISNULL(Y.PONTUACAO,37),Y.TEMPO,Y.SALA,DTC,DESCONHECIDO,
                            Y.ID_PESSOANEXT,
                            ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = Y.PAC AND E.ESUP = @REC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 GROUP BY PC.PAC),0) QCH,Y.DAM,Y.NPFMPS,SQA,APL
                            FROM @TPAC Y ,DBO.F001_SPLIT(@ESPSELECIONADA,'|') ES
                            WHERE Y.TEMPO <= 1440 AND  EXISTS (SELECT 1 FROM PACIENTES_INTERCONSULTAS P WHERE Y.PAC=P.PAC) AND ES.ITEM=Y.ESP
                            
                            UNION ALL
                            
                            SELECT TOP 501 Y.PAC,Y.NPF,Y.NOME,Y.DNS,Y.DAT,Y.DESP,Y.NMPS,Y.SEQ,Y.DPA,Y.DSX,Y.TIPOATEND,Y.ESPERA,Y.ID_PRI,Y.MPS,Y.SENHA,ISNULL(Y.PONTUACAO,37),Y.TEMPO,Y.SALA,DTC,DESCONHECIDO,
                            Y.ID_PESSOANEXT,
                            ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = Y.PAC AND E.ESUP = @REC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 GROUP BY PC.PAC),0) QCH,Y.DAM,Y.NPFMPS,SQA,APL
                            FROM @TPAC Y,DBO.F001_SPLIT(@ESPSELECIONADA,'|') ES
                            WHERE Y.TEMPO <= 1440 AND EXISTS (SELECT 1 FROM PACIENTES_INTERCONSULTAS P WHERE Y.PAC=P.PAC AND P.MPS=@MPS AND P.DT_AGENDA=GETDATE()) AND ES.ITEM=Y.ESP 
                            RETURN
                            END
                            ELSE IF  @OP = 3
                            BEGIN 
                            SELECT TOP 501 PAC,NPF,NOME,DNS,DAT,DESP,NMPS,SEQ,DPA,DSX,TIPOATEND,ESPERA,ISNULL(ID_PRI,99) ID_PRI,MPS,SENHA,ISNULL(PONTUACAO,37) PONTUACAO,TEMPO,SALA,DTC, DESCONHECIDO,
                            Y.ID_PESSOANEXT,
                            ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = Y.PAC AND E.ESUP = @REC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 GROUP BY PC.PAC),0) QCH,DAM,NPFMPS,SQA,APL
                            FROM @TPAC Y,DBO.F001_SPLIT(@ESPSELECIONADA,'|') ES
                            WHERE Y.TEMPO <= 1440 AND ES.ITEM=Y.ESP AND nmps is not null
                            ORDER BY PONTUACAO DESC,TEMPO DESC
                            END
                            ELSE IF @OP = 4
                            BEGIN
                            SELECT TOP 501 PAC,NPF,NOME,DNS,DAT,DESP,NMPS,SEQ,DPA,DSX,TIPOATEND,ESPERA,ISNULL(ID_PRI,99) ID_PRI,MPS,SENHA,ISNULL(PONTUACAO,37) PONTUACAO,TEMPO,SALA,DTC, DESCONHECIDO,
                            Y.ID_PESSOANEXT,ISNULL((SELECT COUNT(*) 
                            FROM PAINEL_ELETRONICO_CHAMADAS PC 
                            INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS 
                            INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = Y.PAC AND E.ESUP = @REC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 GROUP BY PC.PAC),0) QCH,DAM,NPFMPS,SQA,APL
                            FROM @TPAC Y,DBO.F001_SPLIT(@ESPSELECIONADA,'|') ES
                            WHERE Y.TEMPO <= 1440 AND ES.ITEM=Y.ESP  
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = Y.PAC AND E.ESUP = @REC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 GROUP BY PC.PAC),0) >= (SELECT VALOR FROM Eorg_Parametros WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@REC)
                            ORDER BY PONTUACAO DESC,TEMPO DESC
                            END
                            RETURN
                            END
                            
                            ELSE IF @CONTADOR IS NOT NULL
                            BEGIN
                            DECLARE @VERMELHO INT,@AMARELO INT, @VERDE INT, @AZUL INT, @LARANJA INT
                            DECLARE @EORGTRIAGEM_T TABLE (ITEM INT)
                            DECLARE @EORGACOLHIMENTO_T TABLE (ITEM INT)
                            
                            SET @IDREC=(SELECT ID FROM EORG WHERE EORG=@CONTADOR)
                            
                            IF @IDREC=102
                            BEGIN
                            SELECT @REC=ESUP,@RECORIGINAL=EORG FROM EORG WHERE EORG=@CONTADOR
                            END
                            
                            INSERT INTO @EORGTRIAGEM_T(ITEM)
                            SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@CONTADOR AND ID=101
                            
                            INSERT INTO @EORGACOLHIMENTO_T(ITEM)
                            SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@CONTADOR AND ID=100
                            
                            --SET @EORGACOLHIMENTO=(SELECT ISNULL(EORG,0) FROM EORG WHERE ESUP=@CONTADOR AND ID=100)
                            
                            IF EXISTS(SELECT 1 FROM EORG E,EORG ES1,EORG ES2 WHERE E.ESUP=ES1.EORG AND ES1.ESUP=ES2.EORG AND E.EORG=@CONTADOR AND (E.ID=4 OR ES1.ID=4 OR ES2.ID=4))
                            BEGIN
                            
                            SET @DRF=DATEADD(HOUR,-72,GETDATE())
                            END
                            ELSE IF EXISTS(SELECT 1 FROM EORG E,EORG ES1,EORG ES2 WHERE E.ESUP=ES1.EORG AND ES1.ESUP=ES2.EORG AND E.EORG=@CONTADOR AND (E.ID=3 OR ES1.ID=3 OR ES2.ID=3))
                            BEGIN
                            
                            SET @DRF=DATEADD(HOUR,-24,GETDATE())
                            END
                            ELSE
                            BEGIN
                            
                            SET @DRF=DATEADD(YEAR,-10,GETDATE())
                            END
                            
                            SELECT @VERMELHO=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG IN(SELECT T.ITEM FROM @EORGTRIAGEM_T T) OR (PP.EORG IN(SELECT ITEM FROM @EORGACOLHIMENTO_T)))
                            AND PP.Id_Pri=1
                            AND DATEDIFF(HOUR,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>=0
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = P.PAC AND E.ESUP = @CONTADOR AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                             AND P.IAC IS NULL
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND PP.DTA>=@DRF
                            
                            
                            SELECT @LARANJA=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG IN(SELECT T.ITEM FROM @EORGTRIAGEM_T T) OR (PP.EORG IN(SELECT ITEM FROM @EORGACOLHIMENTO_T)))
                            AND PP.Id_Pri=2
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoLaranja}
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = P.PAC AND E.ESUP = SP.Loc AND SP.ST=1 AND ISNULL(PC.ST,1)=1 AND P.IAC IS NULL
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND PP.DTA>=@DRF
                            
                            SELECT @AMARELO=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG IN(SELECT T.ITEM FROM @EORGTRIAGEM_T T) OR (PP.EORG IN(SELECT ITEM FROM @EORGACOLHIMENTO_T)))
                            AND PP.Id_Pri=3
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoAmarelo}
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = P.PAC AND E.ESUP = @CONTADOR AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND PP.DTA>=@DRF
                            
                            
                            SELECT @VERDE=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG IN(SELECT T.ITEM FROM @EORGTRIAGEM_T T) OR (PP.EORG IN(SELECT ITEM FROM @EORGACOLHIMENTO_T)))
                            AND PP.Id_Pri=4
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoVerde}
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = P.PAC AND E.ESUP = @CONTADOR AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND PP.DTA>=@DRF
                            
                            
                            SELECT @AZUL=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG IN(SELECT T.ITEM FROM @EORGTRIAGEM_T T) OR (PP.EORG IN(SELECT ITEM FROM @EORGACOLHIMENTO_T)))
                            AND PP.Id_Pri=5
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoAzul}
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS INNER JOIN EORG E ON PC.LOC=E.EORG 
                            WHERE PC.PAC = P.PAC AND E.ESUP = @CONTADOR AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=45 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND PP.DTA>=@DRF
                            
                            DECLARE @T_C TABLE(VERMELHO INT, LARANJA INT, AMARELO INT, VERDE INT, AZUL INT)
                            
                            INSERT INTO @T_C(VERMELHO, LARANJA, AMARELO, VERDE, AZUL)
                            SELECT 0, 0, 0, 0, 0
                            
                            --================VERMELHO 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE EC.EORG = @CONTADOR AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 1 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET VERMELHO = 1
                            END
                            --================LARANJA 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE EC.EORG = @CONTADOR  AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 2 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET LARANJA = 1
                            END
                            --================AMARELO 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE  EC.EORG = @CONTADOR  AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 3 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET AMARELO = 1
                            END
                            --================VERDE 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE  EC.EORG = @CONTADOR  AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 4 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET VERDE = 1
                            END
                            --================AZUL 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE  EC.EORG = @CONTADOR  AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 5 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET AZUL = 1
                            END
                            
                            SELECT CASE WHEN C.VERMELHO = 1 THEN ISNULL(@VERMELHO,0) ELSE NULL END VERMELHO,
                                   CASE WHEN C.AMARELO = 1 THEN ISNULL(@AMARELO,0) ELSE NULL END AMARELO,
                                   CASE WHEN C.VERDE = 1 THEN ISNULL(@VERDE,0) ELSE NULL END VERDE,
                                   CASE WHEN C.AZUL = 1 THEN ISNULL(@AZUL,0) ELSE NULL END AZUL,
                                   CASE WHEN C.LARANJA = 1 THEN ISNULL(@LARANJA,0) ELSE NULL END LARANJA
                            FROM @T_C C
                            
                            END";

            List<Hashtable> list = GetList(SqlCmd, ConnectionString);

            SqlCmd = $@"ALTER PROC DBO.P270_PACIENTESTEMPOEXCEDIDOS
                            @REC INT=NULL,
                            @RECEPCAO TINYINT=NULL,
                            @CONTADOR INT=NULL,
                            @ID_PRI INT=NULL
                            
                            AS
                            SET NOCOUNT ON
                            SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
                            
                            DECLARE @IDREC INT,@ESUP INT
                            DECLARE @DRF DATETIME
                            
                            IF @REC IS NOT NULL
                            BEGIN
                            
                            SELECT @IDREC=ID,@ESUP=ESUP FROM EORG WHERE EORG=@REC
                            
                            IF EXISTS(SELECT 1 FROM EORG E,EORG ES1,EORG ES2 WHERE E.ESUP=ES1.EORG AND ES1.ESUP=ES2.EORG AND E.EORG=@REC AND (E.ID=4 OR ES1.ID=4 OR ES2.ID=4))
                            BEGIN
                            SET @DRF=DATEADD(HOUR,-72,GETDATE())
                            END
                            ELSE IF EXISTS(SELECT 1 FROM EORG E,EORG ES1,EORG ES2 WHERE E.ESUP=ES1.EORG AND ES1.ESUP=ES2.EORG AND E.EORG=@REC AND (E.ID=3 OR ES1.ID=3 OR ES2.ID=3))
                            BEGIN
                            SET @DRF=DATEADD(HOUR,-24,GETDATE())
                            END
                            ELSE
                            BEGIN
                            SET @DRF=DATEADD(YEAR,-10,GETDATE())
                            END
                            
                            DECLARE @AM TABLE(PAC INT, ID INT)
                            DECLARE @VE TABLE(PAC INT, ID INT)
                            DECLARE @AZ TABLE(PAC INT, ID INT)
                            DECLARE @VER TABLE(PAC INT, ID INT)
                            DECLARE @LA TABLE(PAC INT, ID INT)
                            
                            INSERT INTO @AM(PAC, ID)
                            SELECT PP.PAC, 3
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL) 
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=3
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoAmarelo}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro) 
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            INSERT INTO @VE(PAC, ID)
                            SELECT PP.Pac, 4
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=4
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoVerde}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro) 
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            INSERT INTO @AZ(PAC, ID)
                            SELECT PP.PAC, 5
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=5
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoAzul}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro) 
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            INSERT INTO @VER(PAC, ID)
                            SELECT PP.Pac,1
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=1
                            AND DATEDIFF(HOUR,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>=0
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro) 
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            INSERT INTO @LA(PAC, ID)
                            SELECT PP.PAC,2
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@REC OR P.LOC = @REC)
                            AND PP.Id_Pri=2
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoLaranja}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            
                            SELECT DISTINCT P.PAC,ISNULL(PF.NPF,PP.NPF) NPF,
                            UPPER(CASE WHEN PP.NPF = (SELECT TOP 1 NPF_GENERICO FROM CONFIGURACAO_01) THEN ISNULL((SELECT PA1.NOMEDESCONHECIDO FROM PACIENTES PA1 WHERE PA1.PAC = PP.PAC),PP.NOME)
                            ELSE PF.NOME END) NOME,DBO.F001_CALCULA_IDADE(ISNULL(PP.DNS,PF.DNS),2) DNS,
                            P.DAT,P.DT_CH DTC,S.DAB DESP,
                            P.MPS,M.NOME NMPS,P.SEQ,CONVERT(VARCHAR,P.SEQ) SENHA,P.DPA,X.DESCRICAO DSX,ISNULL(PP.ID_PRI,99) ID_PRI,CC.NPF_GENERICO, PP.PONTUACAO,
                            PP.Dt_Final DATA_TRIAGEM, CASE WHEN PHC.Dt = PP.Dt_Final THEN NULL ELSE PHC.Dt END DATA_RETRIAGEM
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.Pac = PP.Pac AND PHC.ChvPHC = (SELECT TOP 1 MAX(PHCC.CHVPHC) FROM Pacientes_Historico_Classificacao PHCC WHERE PHCC.Pac = PP.Pac)
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.Iac IS NULL
                            LEFT JOIN CORPOCLINICO C ON C.MPS=P.MPS 
                            LEFT JOIN PESSOA_FISICA M ON M.NPF=C.NPF 
                            LEFT JOIN PACIENTES_TIPA PT ON PT.TIPA=P.TIPA
                            LEFT JOIN PESSOA_FISICA PF ON PF.NPF=P.NPF AND PF.NPF<>(SELECT ISNULL(NPF_GENERICO,0) FROM CONFIGURACAO_01) 
                            LEFT JOIN PRONTUARIOS PR ON PR.NPF=PF.NPF 
                            LEFT JOIN SEXO X ON X.SX=ISNULL(PF.SX,PP.SX)
                            LEFT JOIN PAINEL_ELETRONICO_SENHAS PES ON PES.PAC = P.PAC
                            LEFT JOIN PESSOA_FISICA_CNS FC ON FC.NPF = PF.NPF AND FC.ST = 1
                            LEFT JOIN NUMERO_CIDADAO NC ON NC.NPF = PF.NPF,PRODUTOS_SERVICOS S,CONFIGURACAO_01 CC
                            WHERE P.ESP=S.COD AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND PP.EORG=@REC AND PP.ID_PRI=@ID_PRI 
                            AND (
                            EXISTS(SELECT 1 FROM @AM I WHERE I.PAC = PP.PAC AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VE I WHERE I.PAC = PP.PAC AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @AZ I WHERE I.PAC = PP.PAC AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @VER I WHERE I.PAC = PP.PAC AND I.ID = @ID_PRI) OR
                            EXISTS(SELECT 1 FROM @LA I WHERE I.PAC = PP.PAC AND I.ID = @ID_PRI)
                            )
                            
                            AND ISNULL(K.PAC,0)=CASE WHEN @RECEPCAO=1 THEN K.PAC ELSE 0 END
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, Protocolos_Gerais PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.Apl = 45 AND PR.ChvPro = PG.ChvPro)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@REC)
                            AND PP.DTA>=@DRF
                            ORDER BY P.DAT DESC
                            
                            RETURN
                            END
                            ELSE IF @CONTADOR IS NOT NULL
                            BEGIN
                            DECLARE @VERMELHO INT,@AMARELO INT, @VERDE INT, @AZUL INT, @LARANJA INT
                            
                            SELECT @IDREC=ID,@ESUP=ESUP FROM EORG WHERE EORG=@CONTADOR
                            
                            IF EXISTS(SELECT 1 FROM EORG E,EORG ES1,EORG ES2 WHERE E.ESUP=ES1.EORG AND ES1.ESUP=ES2.EORG AND E.EORG=@CONTADOR AND (E.ID=4 OR ES1.ID=4 OR ES2.ID=4))
                            BEGIN
                            SET @DRF=DATEADD(HOUR,-72,GETDATE())
                            END
                            ELSE IF EXISTS(SELECT 1 FROM EORG E,EORG ES1,EORG ES2 WHERE E.ESUP=ES1.EORG AND ES1.ESUP=ES2.EORG AND E.EORG=@CONTADOR AND (E.ID=3 OR ES1.ID=3 OR ES2.ID=3))
                            BEGIN
                            SET @DRF=DATEADD(HOUR,-24,GETDATE())
                            END
                            ELSE
                            BEGIN
                            SET @DRF=DATEADD(YEAR,-10,GETDATE())
                            END
                            
                            SELECT @VERMELHO=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@CONTADOR OR P.LOC = @CONTADOR)
                            AND PP.ID_PRI=1
                            AND DATEDIFF(HOUR,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>=0
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, PROTOCOLOS_GERAIS PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.APL = 45 AND PR.CHVPRO = PG.CHVPRO)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND PP.DTA>=@DRF
                            
                            SELECT @LARANJA=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@CONTADOR OR P.LOC = @CONTADOR)
                            AND PP.ID_PRI=2
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoLaranja}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, PROTOCOLOS_GERAIS PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.APL = 45 AND PR.CHVPRO = PG.CHVPRO)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND PP.DTA>=@DRF
                            
                            SELECT @AMARELO=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL) 
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@CONTADOR OR P.LOC = @CONTADOR)
                            AND PP.ID_PRI=3
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoAmarelo}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, PROTOCOLOS_GERAIS PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.APL = 45 AND PR.CHVPRO = PG.CHVPRO)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND PP.DTA>=@DRF
                            
                            SELECT @VERDE=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@CONTADOR OR P.LOC = @CONTADOR)
                            AND PP.ID_PRI=4
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoVerde}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, PROTOCOLOS_GERAIS PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.APL = 45 AND PR.CHVPRO = PG.CHVPRO)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND PP.DTA>=@DRF
                            
                            SELECT @AZUL=ISNULL(COUNT(*),0)
                            FROM PACIENTES_TRIAGEM PP 
                            LEFT JOIN FATURAMENTO_CONTAS K ON K.PAC=PP.PAC
                            INNER JOIN PACIENTES P ON PP.PAC=P.PAC AND P.IAC IS NULL
                            LEFT JOIN PACIENTES_HISTORICO_CLASSIFICACAO PHC ON PHC.PAC = PP.PAC AND PHC.CHVPHC = (SELECT TOP 1 PHCC.CHVPHC FROM PACIENTES_HISTORICO_CLASSIFICACAO PHCC WHERE PHCC.PAC = PP.PAC ORDER BY PHCC.DT ASC)
                            WHERE DATEDIFF(MINUTE,ISNULL(PHC.DT,P.DAT),GETDATE()) <= 1440 AND (P.DST IS NULL OR P.DAM IS NULL)
                            AND PP.DT_FINAL IS NOT NULL AND (PP.EORG=@CONTADOR OR P.LOC = @CONTADOR)
                            AND PP.ID_PRI=5
                            AND DATEDIFF(MINUTE,ISNULL(PHC.DT,PP.DT_FINAL),GETDATE())>={TempoAzul}
                            AND NOT EXISTS (SELECT 1 
                            FROM PEP_PACIENTES PPP,PROTOCOLOS_RECURSOS PR, PROTOCOLOS_GERAIS PG 
                            WHERE PPP.PAC=PP.PAC AND PR.CHVREC=PPP.CHVREC AND PG.APL = 45 AND PR.CHVPRO = PG.CHVPRO)
                            AND ISNULL((SELECT COUNT(*) FROM PAINEL_ELETRONICO_CHAMADAS PC INNER JOIN PAINEL_ELETRONICO_SENHAS SP ON SP.CHVS=PC.CHVS
                            WHERE PC.PAC = P.PAC AND SP.ST=1 AND ISNULL(PC.ST,1)=1 
                            GROUP BY PC.PAC),0) < (SELECT VALOR FROM EORG_PARAMETROS WHERE APL=270 AND SGL='NCHAMADAS' AND EORG=@CONTADOR)
                            AND PP.DTA>=@DRF
                            
                            DECLARE @T_C TABLE(VERMELHO INT, LARANJA INT, AMARELO INT, VERDE INT, AZUL INT)
                            
                            INSERT INTO @T_C(VERMELHO, LARANJA, AMARELO, VERDE, AZUL)
                            SELECT 0, 0, 0, 0, 0
                            
                            --================VERMELHO 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE (EC.EORG = (SELECT TOP 1 ESUP FROM EORG WHERE EORG = @CONTADOR) OR EC.EORG = @CONTADOR)  AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 1 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET VERMELHO = 1
                            END
                            --================LARANJA 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE (EC.EORG = (SELECT TOP 1 ESUP FROM EORG WHERE EORG = @CONTADOR) OR EC.EORG = @CONTADOR) AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 2 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET LARANJA = 1
                            END
                            --================AMARELO 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE (EC.EORG = (SELECT TOP 1 ESUP FROM EORG WHERE EORG = @CONTADOR) OR EC.EORG = @CONTADOR) AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 3 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET AMARELO = 1
                            END
                            --================VERDE 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE (EC.EORG = (SELECT TOP 1 ESUP FROM EORG WHERE EORG = @CONTADOR) OR EC.EORG = @CONTADOR) AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 4 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET VERDE = 1
                            END
                            --================AZUL 
                            IF EXISTS(SELECT 1 FROM EORG_CLASSIFICACAORISCO EC, TABELA_USO_GERAL TUG 
                            WHERE (EC.EORG = (SELECT TOP 1 ESUP FROM EORG WHERE EORG = @CONTADOR) OR EC.EORG = @CONTADOR) AND EC.CHVG = TUG.CHVG AND TUG.CDG=355 AND TUG.ORD = 5 
                            AND EC.Id_Visivel = 1)
                            BEGIN
                            	UPDATE @T_C SET AZUL = 1
                            END
                            
                            SELECT CASE WHEN C.VERMELHO = 1 THEN ISNULL(@VERMELHO,0) ELSE NULL END VERMELHO,
                                   CASE WHEN C.AMARELO = 1 THEN ISNULL(@AMARELO,0) ELSE NULL END AMARELO,
                                   CASE WHEN C.VERDE = 1 THEN ISNULL(@VERDE,0) ELSE NULL END VERDE,
                                   CASE WHEN C.AZUL = 1 THEN ISNULL(@AZUL,0) ELSE NULL END AZUL,
                                   CASE WHEN C.LARANJA = 1 THEN ISNULL(@LARANJA,0) ELSE NULL END LARANJA
                            FROM @T_C C
                            
                            END";
            Execute(SqlCmd, ConnectionString);
        }
        public static Boolean Execute(String SqlCmd, String ConnectionString)
        {
            try
            {

                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand(SqlCmd, sqlConnection))
                    {
                        sqlCommand.CommandText = SqlCmd;
                        sqlCommand.CommandTimeout = 360;

                        sqlConnection.Open();

                        SqlCommand command = new SqlCommand(SqlCmd, sqlConnection);

                        command.Parameters.Clear();
                        command.CommandTimeout = 360;

                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " - " + ex.StackTrace);

                return false;
            }
            finally
            {
                GC.Collect();
            }
        }

        public static List<Hashtable> GetList(String SqlCmd, String ConnectionString)
        {
            List<Hashtable> listHashtable = null;
            Hashtable hashtable = null;

            try
            {
                listHashtable = new List<Hashtable>();

                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand(SqlCmd, sqlConnection))
                    {
                        sqlCommand.CommandText = SqlCmd;
                        sqlCommand.CommandTimeout = 360;

                        sqlConnection.Open();

                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            listHashtable = new List<Hashtable>();

                            while (sqlDataReader.Read())
                            {
                                hashtable = new Hashtable();

                                for (int i = 0; i < sqlDataReader.FieldCount; i++)
                                {
                                    hashtable.Add(sqlDataReader.GetName(i), sqlDataReader[i]);
                                }

                                listHashtable.Add(hashtable);
                            }
                        }

                        sqlConnection.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " - " + ex.StackTrace);

                listHashtable = null;
            }
            finally
            {
                hashtable = null;

                GC.Collect();
            }

            return listHashtable;
        }
    }
}
