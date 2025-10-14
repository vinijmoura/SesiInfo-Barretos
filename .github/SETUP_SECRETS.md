# GitHub Secrets Setup Guide

Este guia explica como configurar os secrets necessários para o pipeline CI/CD no GitHub Actions.

## 📋 Secrets Necessários

### Azure Credentials

#### 1. AZURE_CLIENT_ID
- **Descrição**: ID do cliente do Service Principal
- **Como obter**: Veja seção "Criando Service Principal"

#### 2. AZURE_CLIENT_SECRET
- **Descrição**: Secret do Service Principal
- **Como obter**: Veja seção "Criando Service Principal"

#### 3. AZURE_SUBSCRIPTION_ID
- **Descrição**: ID da sua subscrição Azure
- **Como obter**: 
```bash
az account show --query id --output tsv
```

#### 4. AZURE_TENANT_ID
- **Descrição**: ID do tenant Azure AD
- **Como obter**:
```bash
az account show --query tenantId --output tsv
```

#### 5. AZURE_CREDENTIALS
- **Descrição**: JSON completo com todas as credenciais
- **Como obter**: Veja seção "Criando Service Principal"

### Terraform Backend

#### 6. TERRAFORM_BACKEND_RG
- **Descrição**: Resource Group onde está o storage do backend do Terraform
- **Exemplo**: `rg-terraform-state`

#### 7. TERRAFORM_BACKEND_SA
- **Descrição**: Nome do Storage Account do backend do Terraform
- **Exemplo**: `sttfstate123456789`

#### 8. TERRAFORM_BACKEND_CONTAINER
- **Descrição**: Nome do container no Storage Account
- **Exemplo**: `tfstate`

## 🔧 Criando Service Principal

### Passo 1: Login no Azure

```bash
az login
```

### Passo 2: Obter Subscription ID

```bash
az account show --query id --output tsv
```

### Passo 3: Criar Service Principal

```bash
az ad sp create-for-rbac \
  --name "sesiinfo-github-actions" \
  --role contributor \
  --scopes /subscriptions/{SUBSCRIPTION_ID} \
  --sdk-auth
```

**Substitua `{SUBSCRIPTION_ID}` pelo ID obtido no Passo 2.**

### Passo 4: Salvar Output

O comando acima retornará um JSON similar a:

```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

**Importante**: Salve este JSON completo - você vai precisar dele!

## 🔐 Configurando Secrets no GitHub

### Passo 1: Acessar Configurações

1. Vá até o repositório no GitHub
2. Clique em **Settings**
3. No menu lateral, clique em **Secrets and variables** > **Actions**
4. Clique em **New repository secret**

### Passo 2: Adicionar Cada Secret

Para cada secret listado acima:

1. Clique em **New repository secret**
2. Em **Name**, digite o nome exato do secret (ex: `AZURE_CLIENT_ID`)
3. Em **Value**, cole o valor correspondente
4. Clique em **Add secret**

### Valores dos Secrets

Do JSON obtido no Service Principal:

- `AZURE_CLIENT_ID` = valor de `clientId`
- `AZURE_CLIENT_SECRET` = valor de `clientSecret`
- `AZURE_SUBSCRIPTION_ID` = valor de `subscriptionId`
- `AZURE_TENANT_ID` = valor de `tenantId`
- `AZURE_CREDENTIALS` = o JSON completo

## 🏗️ Configurando Backend do Terraform

### Passo 1: Criar Resource Group

```bash
RESOURCE_GROUP="rg-terraform-state"
LOCATION="eastus"

az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

### Passo 2: Criar Storage Account

```bash
STORAGE_ACCOUNT="sttfstate$(date +%s)"

az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS \
  --encryption-services blob \
  --min-tls-version TLS1_2
```

### Passo 3: Criar Container

```bash
CONTAINER_NAME="tfstate"

az storage container create \
  --name $CONTAINER_NAME \
  --account-name $STORAGE_ACCOUNT \
  --auth-mode login
```

### Passo 4: Configurar Secrets

Use os valores criados acima:

- `TERRAFORM_BACKEND_RG` = valor de `$RESOURCE_GROUP`
- `TERRAFORM_BACKEND_SA` = valor de `$STORAGE_ACCOUNT`
- `TERRAFORM_BACKEND_CONTAINER` = valor de `$CONTAINER_NAME`

## ✅ Verificação

### Verificar Secrets no GitHub

1. Vá em Settings > Secrets and variables > Actions
2. Você deve ver todos os 8 secrets listados

### Testar Conexão Azure

```bash
az login --service-principal \
  -u $AZURE_CLIENT_ID \
  -p $AZURE_CLIENT_SECRET \
  --tenant $AZURE_TENANT_ID

az account show
```

### Verificar Backend do Terraform

```bash
az storage container show \
  --name $CONTAINER_NAME \
  --account-name $STORAGE_ACCOUNT \
  --auth-mode login
```

## 🔒 Segurança

### Boas Práticas

1. **Nunca compartilhe os secrets**: São credenciais sensíveis
2. **Rotacione regularmente**: Troque os secrets periodicamente
3. **Princípio do menor privilégio**: Service Principal com apenas as permissões necessárias
4. **Monitore o uso**: Configure alertas no Azure
5. **Use ambientes do GitHub**: Configure proteções adicionais

### Rotação de Secrets

Para rotacionar o Service Principal:

```bash
# Resetar credenciais
az ad sp credential reset \
  --name "sesiinfo-github-actions" \
  --create-cert false
```

Atualize os secrets no GitHub com os novos valores.

## 🐛 Troubleshooting

### Erro: Invalid client secret

- Verifique se copiou corretamente o `clientSecret`
- Certifique-se de não ter espaços extras
- Tente recriar o Service Principal

### Erro: Insufficient privileges

```bash
# Dar permissões de contributor
az role assignment create \
  --assignee $AZURE_CLIENT_ID \
  --role Contributor \
  --scope /subscriptions/$AZURE_SUBSCRIPTION_ID
```

### Erro: Storage account not found

- Verifique o nome do Storage Account (sem espaços)
- Confirme que o Service Principal tem acesso ao Storage Account

## 📚 Referências

- [GitHub Encrypted Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [Azure Service Principals](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli)
- [Terraform Azure Backend](https://www.terraform.io/docs/language/settings/backends/azurerm.html)

## 💡 Dicas

1. Use um gerenciador de senhas para guardar os valores dos secrets temporariamente
2. Documente quando foi criado cada secret para facilitar rotação
3. Configure notificações de deploy no GitHub para monitorar execuções
4. Teste o pipeline em um repositório de teste primeiro
