#language: pt-BR

Funcionalidade: Criar ordem de serviço completa
    Como administrador do sistema
    Eu quero criar uma ordem de serviço completa incluindo cliente, veículo, serviços e itens
    Para facilitar o cadastro inicial de novas ordens de serviço

Cenário: Registrar ordem de serviço com cliente e veículo novos
    Dado que sou um administrador autenticado
    E que o cliente informado ainda não possui cadastro
    E que o veículo informado ainda não possui cadastro
    E que o sistema deve cadastrar o novo cliente automaticamente
    E que o sistema deve cadastrar o novo veículo automaticamente
    Quando eu solicitar a criação da ordem de serviço completa
    Então a ordem de serviço deve ser registrada com sucesso
    E o sistema deve informar onde acessar a ordem criada
    E o status inicial deve ser "Recebida"
    E um código identificador deve ser gerado com prefixo "OS-"
    E a ordem de serviço deve estar registrada no sistema

Cenário: Negar criação quando o solicitante não é administrador
    Dado que sou um cliente comum autenticado
    Quando eu solicitar a criação da ordem de serviço completa
    Então o acesso deve ser negado
    E o sistema deve informar que apenas administradores podem criar ordens de serviço completas

Cenário: Reaproveitar cadastro de cliente e veículo já existentes
    Dado que sou um administrador autenticado
    E que o cliente informado já possui cadastro
    E que o veículo informado já possui cadastro
    E que o sistema não deve duplicar o cadastro do cliente
    E que o sistema não deve duplicar o cadastro do veículo
    Quando eu solicitar a criação da ordem de serviço completa
    Então a ordem de serviço deve ser registrada com sucesso
    E a ordem de serviço deve estar registrada no sistema

Cenário: Incluir apenas serviços e itens válidos na ordem de serviço
    Dado que sou um administrador autenticado
    E que o cliente e o veículo já possuem cadastro
    E que informei 2 serviços mas apenas 1 existe no catálogo
    E que informei 2 itens de estoque mas apenas 1 existe no estoque
    Quando eu solicitar a criação da ordem de serviço completa
    Então a ordem de serviço deve ser registrada com sucesso
    E apenas os serviços válidos devem ser incluídos na ordem
    E apenas os itens válidos devem ser incluídos na ordem
