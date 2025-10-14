# Terraform Infrastructure

Este diretório contém a infraestrutura como código (IaC) para provisionar recursos Azure necessários para a aplicação SesiInfo-Barretos.

## Recursos Provisionados

- **Resource Group**: Contêiner lógico para recursos Azure
- **Storage Account**: Armazenamento necessário para Function App
- **App Service Plan**: Plano de hospedagem (Consumption/Serverless)
- **Linux Function App**: Aplicação .NET 9 isolada
- **Application Insights**: Monitoramento e telemetria

## Estrutura de Arquivos

- `main.tf` - Configuração do provider e backend
- `variables.tf` - Definição de variáveis
- `outputs.tf` - Outputs do Terraform
- `resources.tf` - Definição de recursos Azure
- `dev.tfvars.example` - Exemplo de variáveis para DEV
- `prod.tfvars.example` - Exemplo de variáveis para PROD

## Pré-requisitos

1. **Terraform** instalado (>= 1.0)
2. **Azure CLI** instalado e autenticado
3. **Service Principal** com permissões adequadas
4. **Backend do Terraform** configurado (Storage Account)

## Configuração Inicial

### 1. Configure o Backend do Terraform

Primeiro, crie os recursos para o backend do Terraform:

```bash
# Variáveis
RESOURCE_GROUP="rg-terraform-state"
STORAGE_ACCOUNT="sttfstate$(date +%s)"
CONTAINER_NAME="tfstate"
LOCATION="eastus"

# Criar Resource Group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Criar Storage Account
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS

# Criar Container
az storage container create \
  --name $CONTAINER_NAME \
  --account-name $STORAGE_ACCOUNT
```

### 2. Configure as Variáveis de Ambiente

```bash
export ARM_CLIENT_ID="<service-principal-client-id>"
export ARM_CLIENT_SECRET="<service-principal-secret>"
export ARM_SUBSCRIPTION_ID="<azure-subscription-id>"
export ARM_TENANT_ID="<azure-tenant-id>"
```

### 3. Crie os Arquivos de Variáveis

```bash
# DEV
cp dev.tfvars.example dev.tfvars
# Edite dev.tfvars conforme necessário

# PROD
cp prod.tfvars.example prod.tfvars
# Edite prod.tfvars conforme necessário
```

## Uso

### Inicializar Terraform

```bash
# Para DEV
terraform init \
  -backend-config="resource_group_name=$RESOURCE_GROUP" \
  -backend-config="storage_account_name=$STORAGE_ACCOUNT" \
  -backend-config="container_name=$CONTAINER_NAME" \
  -backend-config="key=dev.terraform.tfstate"

# Para PROD
terraform init \
  -backend-config="resource_group_name=$RESOURCE_GROUP" \
  -backend-config="storage_account_name=$STORAGE_ACCOUNT" \
  -backend-config="container_name=$CONTAINER_NAME" \
  -backend-config="key=prod.terraform.tfstate"
```

### Planejar Mudanças

```bash
# DEV
terraform plan -var-file="dev.tfvars"

# PROD
terraform plan -var-file="prod.tfvars"
```

### Aplicar Mudanças

```bash
# DEV
terraform apply -var-file="dev.tfvars"

# PROD
terraform apply -var-file="prod.tfvars"
```

### Destruir Recursos (cuidado!)

```bash
# DEV
terraform destroy -var-file="dev.tfvars"

# PROD
terraform destroy -var-file="prod.tfvars"
```

## Variáveis

### Obrigatórias

- `environment`: Nome do ambiente (dev, prod)

### Opcionais

- `location`: Região Azure (default: "East US")
- `app_name`: Nome da aplicação (default: "sesiinfo")
- `resource_group_name`: Nome customizado para RG (default: auto-gerado)
- `tags`: Tags adicionais para recursos

## Outputs

Após o apply, os seguintes outputs estarão disponíveis:

- `function_app_name`: Nome do Function App criado
- `function_app_default_hostname`: URL do Function App
- `function_app_id`: ID do recurso Function App
- `resource_group_name`: Nome do Resource Group

### Visualizar Outputs

```bash
terraform output
terraform output function_app_name
```

## Nomenclatura de Recursos

Os recursos seguem a convenção:

- Resource Group: `rg-{app_name}-{environment}`
- Function App: `func-{app_name}-{environment}`
- Storage Account: `st{app_name}{environment}`
- App Service Plan: `asp-{app_name}-{environment}`
- Application Insights: `appi-{app_name}-{environment}`

## Ambientes

### DEV
- Usado para desenvolvimento e testes
- Deploy automático via GitHub Actions (branch develop/main)
- Storage Account: LRS (Locally Redundant)
- App Service Plan: Consumption (Y1)

### PROD
- Ambiente de produção
- Deploy automático apenas da branch main
- Storage Account: LRS (considere upgrade para GRS)
- App Service Plan: Consumption (Y1, considere Premium para production)

## Segurança

- Nunca commite arquivos `.tfvars` com valores reais
- Use Azure Key Vault para secrets
- Configure Network Security Groups conforme necessário
- Habilite diagnósticos e logging
- Configure RBAC adequadamente

## Troubleshooting

### Erro: Backend not initialized
```bash
terraform init -reconfigure
```

### Erro: State lock
```bash
# Liste locks
az storage container lease list --account-name $STORAGE_ACCOUNT --container-name $CONTAINER_NAME

# Force unlock (use com cautela!)
terraform force-unlock <lock-id>
```

### Erro: Nome já existe
Alguns recursos Azure requerem nomes globalmente únicos. Ajuste `app_name` ou adicione sufixo único.

## Manutenção

### Atualizar Providers

```bash
terraform init -upgrade
```

### Formatar Código

```bash
terraform fmt -recursive
```

### Validar Configuração

```bash
terraform validate
```

## Custo Estimado

### Consumption Plan (Y1)
- **Function App**: ~$0.20/milhão de execuções
- **Storage Account**: ~$0.024/GB/mês
- **Application Insights**: Primeiro 5GB/mês gratuito

**Nota**: Custos podem variar. Consulte a [Calculadora de Preços do Azure](https://azure.microsoft.com/pricing/calculator/).

## Referências

- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Function App Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Terraform Best Practices](https://www.terraform-best-practices.com/)
