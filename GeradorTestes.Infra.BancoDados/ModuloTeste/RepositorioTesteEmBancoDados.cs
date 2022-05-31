using GeradorTestes.Dominio.ModuloDisciplina;
using GeradorTestes.Dominio.Compartilhado;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeradorTestes.Dominio.ModuloTeste;

namespace GeradorTestes.Infra.BancoDados.ModuloTeste
{
    public class RepositorioTesteEmBancoDados : IRepositorioTeste
    {
        private const string enderecoBanco =
         "Data Source=(LocalDB)\\MSSqlLocalDB;" +
         "Initial Catalog=GeradorTestesDb;" +
         "Integrated Security=True;" +
         "Pooling=False";

        #region Sql Queries
        private const string sqlInserir =
            @"INSERT INTO [TBTESTE] 
                (
                    [NOME]
	            )
	            VALUES
                (
                    @NOME
                );SELECT SCOPE_IDENTITY();";

        private const string sqlEditar =
            @"UPDATE [TBTESTE]	
		        SET
			        [NOME] = @NOME
		        WHERE
			        [NUMERO] = @NUMERO";

        private const string sqlExcluir =
            @"DELETE FROM [TBTESTE]
		        WHERE
			        [NUMERO] = @NUMERO";

        private const string sqlSelecionarTodos =
            @"SELECT 
		            [NUMERO], 
		            [NOME]
	            FROM 
		            [TBTESTE]";

        private const string sqlSelecionarPorNumero =
            @"SELECT 
		            [NUMERO], 
		            [NOME]
	            FROM 
		            [TBTESTE]
		        WHERE
                    [NUMERO] = @NUMERO";

        #endregion

        public ValidationResult Inserir(Teste novoTeste)
        {
            var validador = new ValidadorTeste();

            var resultadoValidacao = validador.Validate(novoTeste);

            if (resultadoValidacao.IsValid == false)
                return resultadoValidacao;

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoInsercao = new SqlCommand(sqlInserir, conexaoComBanco);

            ConfigurarParametrosTeste(novoTeste, comandoInsercao);

            conexaoComBanco.Open();
            var id = comandoInsercao.ExecuteScalar();
            novoTeste.Numero = Convert.ToInt32(id);

            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public ValidationResult Editar(Teste teste)
        {
            var validador = new ValidadorTeste();

            var resultadoValidacao = validador.Validate(teste);

            if (resultadoValidacao.IsValid == false)
                return resultadoValidacao;

            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoEdicao = new SqlCommand(sqlEditar, conexaoComBanco);

            ConfigurarParametrosTeste(teste, comandoEdicao);

            conexaoComBanco.Open();
            comandoEdicao.ExecuteNonQuery();
            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public ValidationResult Excluir(Teste teste)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoExclusao = new SqlCommand(sqlExcluir, conexaoComBanco);

            comandoExclusao.Parameters.AddWithValue("NUMERO", teste.Numero);

            conexaoComBanco.Open();
            int numeroRegistrosExcluidos = comandoExclusao.ExecuteNonQuery();

            var resultadoValidacao = new ValidationResult();

            if (numeroRegistrosExcluidos == 0)
                resultadoValidacao.Errors.Add(new ValidationFailure("", "Não foi possível remover o registro"));

            conexaoComBanco.Close();

            return resultadoValidacao;
        }

        public List<Teste> SelecionarTodos()
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarTodos, conexaoComBanco);

            conexaoComBanco.Open();
            SqlDataReader leitorTeste = comandoSelecao.ExecuteReader();

            List<Teste> testes = new List<Teste>();

            while (leitorTeste.Read())
            {
                Teste teste = ConverterParaTeste(leitorTeste);

                testes.Add(teste);
            }

            conexaoComBanco.Close();

            return testes;
        }

        public Teste SelecionarPorNumero(int numero)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarPorNumero, conexaoComBanco);

            comandoSelecao.Parameters.AddWithValue("NUMERO", numero);

            conexaoComBanco.Open();
            SqlDataReader leitorTeste = comandoSelecao.ExecuteReader();

            Teste teste = null;
            if (leitorTeste.Read())
                teste = ConverterParaTeste(leitorTeste);

            conexaoComBanco.Close();

            return teste;
        }

        private static Teste ConverterParaTeste(SqlDataReader leitorTeste)
        {
            int numero = Convert.ToInt32(leitorTeste["NUMERO"]);
            string titulo = Convert.ToString(leitorTeste["TITULO"]);

            var teste = new Teste
            {
                Numero = numero,
                Titulo = titulo,
            };

            return teste;
        }

        private static void ConfigurarParametrosTeste(Teste novoTeste, SqlCommand comando)
        {
            comando.Parameters.AddWithValue("NUMERO", novoTeste.Numero);
            comando.Parameters.AddWithValue("TITULO", novoTeste.Titulo);
        }
    }
}
