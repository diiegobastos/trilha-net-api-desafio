using Microsoft.AspNetCore.Mvc;
using TrilhaApiDesafio.Context;
using TrilhaApiDesafio.Models;
using Microsoft.EntityFrameworkCore;

namespace TrilhaApiDesafio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TarefaController : ControllerBase
    {
        private readonly OrganizadorContext _context;

        public TarefaController(OrganizadorContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            // Buscar o Id no banco de forma assíncrona
            var tarefa = await _context.Tarefas
                .FirstOrDefaultAsync(t => t.Id == id);
            // Validar o tipo de retorno
            if (tarefa == null)
            {
                // Se não encontrar a tarefa, retornar NotFound
                return NotFound(new { Erro = $"Tarefa com ID {id} não encontrada."});
            }
            // Caso contrário, retornar OK com a tarefa encontrada
            return Ok(tarefa);
        }

        [HttpGet("ObterTodos")]
        public async Task<IActionResult> ObterTodos()
        {
            // Buscar todas as tarefas no banco de forma assíncrona
            var tarefas = await _context.Tarefas.ToListAsync();

            return Ok(tarefas);
        }


        [HttpGet("ObterPorTitulo")]
        public async Task<IActionResult> ObterPorTitulo(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
            {
                return BadRequest(new { Erro = "O título não pode estar vazio."});
            }

            // Busca assíncrona das tarefas que contenham o título (case insensitive)
            var tarefas = await _context.Tarefas
                .Where(t => EF.Functions.Like(t.Titulo, $"%{titulo}%"))
                .ToListAsync();

            return Ok(tarefas);
        }

        [HttpGet("ObterPorData")]
        public async Task<IActionResult> ObterPorData(DateTime data)
        {
            var tarefas = await _context.Tarefas
                .Where(x => x.Data.Date == data.Date)
                .ToListAsync();

            return Ok(tarefas);
        }

        [HttpGet("ObterPorStatus")]
        public async Task<IActionResult> ObterPorStatus(EnumStatusTarefa status)
        {
            // Busca assíncrona das tarefas que tenham o status recebido
            var tarefas = await _context.Tarefas
                .Where(x => x.Status == status)
                .ToListAsync();

            return Ok(tarefas);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(Tarefa tarefa)
        {
            if (tarefa.Data == DateTime.MinValue)
                return BadRequest(new { Erro = "A data da tarefa não pode ser vazia" });

            // Adicionar a tarefa recebida no EF
            _context.Tarefas.Add(tarefa);

            // Salvar as mudanças de forma assíncrona
            await _context.SaveChangesAsync();

            // Retornar CreatedAtAction com a rota para obter a tarefa criada
            return CreatedAtAction(nameof(ObterPorId), new { id = tarefa.Id }, tarefa);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, Tarefa tarefa)
        {
            var tarefaBanco = await _context.Tarefas.FindAsync(id);

            if (tarefaBanco == null)
                return NotFound();

            if (tarefa.Data == DateTime.MinValue)
                return BadRequest(new { Erro = "A data da tarefa não pode ser vazia" });

            // Atualiza as informações da tarefaBanco com os dados da tarefa recebida
            tarefaBanco.Titulo = tarefa.Titulo;
            tarefaBanco.Descricao = tarefa.Descricao;
            tarefaBanco.Data = tarefa.Data;
            tarefaBanco.Status = tarefa.Status;

            // Atualiza a tarefa no EF e salvar as mudanças
            _context.Tarefas.Update(tarefaBanco);
            await _context.SaveChangesAsync();

            return Ok(tarefaBanco);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var tarefaBanco = await _context.Tarefas.FindAsync(id);

            if (tarefaBanco == null)
                return NotFound($"Tarefa com id {id} não encontrada.");

            // Remover a tarefa encontrada
            _context.Tarefas.Remove(tarefaBanco);

            // Salvar as mudanças de forma assíncrona
            await _context.SaveChangesAsync();

            // Retorna 204 No Content indicando sucesso sem conteúdo no corpo
            return NoContent();
        }

    }
}
