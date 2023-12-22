# Desafio Final - Transforma Dev (ADA)

## Técnicas de Programação I (C#)

Este projeto foi desenvolvido como parte do programa Trandforma DEV na<a href="https://ada.tech/">ADA</a>, referente ao módulo "Técnicas de Programação I (C#)". O objetivo principal é criar um dashboard.

### Funcionalidades

O sistema é capaz de realizar várias operações relacionadas ao gerenciamento e visualização de dados de um e-commerce. Isso inclui:

- Visualização de resumo de clientes.
- Listagem dos primeiros 100 clientes em ordem alfabética.
- Resumo de pedidos incluindo valor total por mês/ano, top 10 clientes, e total de pedidos por quinzena.
- Listagem dos 30 produtos mais comprados, ordenados por valor e por quantidade.
- Listagem dos 30 produtos mais comprados por categoria e por fornecedor.

### Tecnologias Utilizadas

- **C#**: Linguagem de programação principal.
- **Entity Framework Core**: Usado para o gerenciamento de banco de dados.
- **LINQ**: Para consultas de banco de dados.
- **ASP.NET Core**: Para criar a API REST.

### Estrutura básica do Projeto

- `BancoDeDados`: Contém as entidades do banco de dados e a lógica para carregar dados.
- `DTOs`: Data Transfer Objects para facilitar a transferência de dados.
- `Endpoints`: Definições dos endpoints da API.
- `Program.cs`: Ponto de entrada da aplicação.


### Agradecimento

Gostaríamos de agradecer ao Professor Bruno pela sua dedicação e orientação durante o curso. Sua experiência e conhecimento foram fundamentais para o nosso aprendizado e desenvolvimento neste projeto.