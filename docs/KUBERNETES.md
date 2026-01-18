# Guia de Deploy no Kubernetes

## Índice

- [Pré-requisitos](#pré-requisitos)
- [Deploy Local com Minikube](#deploy-local-com-minikube)
- [Remover Deploy do Minikube](#remover-deploy-do-minikube)
- [Deploy em Produção](#deploy-em-produção)
- [Monitoramento e Troubleshooting](#monitoramento-e-troubleshooting)
- [Configurações Avançadas](#configurações-avançadas)
- [Ambientes (Dev, Staging, Prod)](#ambientes-dev-staging-prod)
- [Segurança](#segurança)
- [CI/CD Integration](#cicd-integration)
- [Recursos Úteis](#recursos-úteis)
- [Problemas Comuns](#problemas-comuns)

---

## Pré-requisitos

- Cluster Kubernetes configurado (GKE, AKS, EKS, etc.)
- `kubectl` configurado e apontando para o cluster correto
- Permissões de administrador no namespace alvo
- Container registry configurado e acessível
- Docker instalado e autenticado no registry

---

## Deploy Local com Minikube

### Opção 1: Script Automatizado (Recomendado)

#### Windows (PowerShell)

```powershell
cd scripts/windows
./minikube-deploy.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
chmod +x minikube-deploy.sh
./minikube-deploy.sh
```

#### Windows (CMD)

```cmd
cd scripts\windows
minikube-deploy.bat
```

### Opção 2: Passo a Passo Manual

#### 1. Iniciar Minikube

```bash
minikube start
```

#### 2. Construir a Imagem Docker

```bash
docker build -t projecttemplate-api:latest -f Dockerfile .
```

#### 3. Carregar Imagem no Minikube

```bash
minikube image load projecttemplate-api:latest
```

#### 4. Aplicar Manifestos Kubernetes

```bash
kubectl apply -k .k8s/
```

#### 5. Verificar Status

```bash
# Ver pods
kubectl get pods -n projecttemplate

# Ver logs
kubectl logs -f deployment/projecttemplate-api -n projecttemplate

# Ver services
kubectl get svc -n projecttemplate
```

#### 6. Acessar a Aplicação

**Opção A: Port Forward**

```bash
kubectl port-forward svc/projecttemplate-api 8080:80 -n projecttemplate
```

Acesse: http://localhost:8080

**Opção B: Minikube Tunnel**

```bash
minikube tunnel
```

Em outro terminal:

```bash
kubectl get svc -n projecttemplate
```

Use o EXTERNAL-IP fornecido.

---

## Remover Deploy do Minikube

### Script Automatizado

#### Windows (PowerShell)

```powershell
cd scripts/windows
./minikube-destroy.ps1
```

#### Linux/macOS

```bash
cd scripts/linux
chmod +x minikube-destroy.sh
./minikube-destroy.sh
```

### Manual

```bash
kubectl delete -k .k8s/
```

---

## Deploy em Produção

### 1. Preparar Imagem Docker

#### Build e Push para Registry

**Build:**

```bash
docker build -t your-registry.com/projecttemplate-api:v1.0.0 -f Dockerfile .
```

**Push:**

```bash
docker push your-registry.com/projecttemplate-api:v1.0.0
```

### 2. Atualizar Manifestos

Atualize `.k8s/deployment.yaml` com a nova imagem:

```yaml
spec:
  template:
    spec:
      containers:
        - name: api
          image: your-registry.com/projecttemplate-api:v1.0.0
          imagePullPolicy: Always
```

### 3. Configurar Secrets (Recomendado)

Crie um Secret para dados sensíveis:

```bash
kubectl create secret generic projecttemplate-secrets \
  --from-literal=database-password=YourSecurePassword \
  --from-literal=jwt-secret=YourJwtSecret \
  -n projecttemplate
```

Atualize o deployment para usar o Secret:

```yaml
env:
  - name: AppSettings__Infrastructure__Database__ConnectionString
    valueFrom:
      secretKeyRef:
        name: projecttemplate-secrets
        key: database-connection-string
```

### 4. Ajustar Recursos

Configure requests/limits em `deployment.yaml`:

```yaml
resources:
  requests:
    cpu: 500m
    memory: 512Mi
  limits:
    cpu: 2000m
    memory: 1Gi
```

### 5. Configurar Réplicas

```yaml
spec:
  replicas: 3
```

### 6. Aplicar no Cluster

#### Criar namespace (primeira vez)

```bash
kubectl create namespace projecttemplate
```

#### Aplicar manifestos

```bash
kubectl apply -k .k8s/
```

---

## Monitoramento e Troubleshooting

### Ver Logs

**Logs em tempo real**

```bash
kubectl logs -f deployment/projecttemplate-api -n projecttemplate
```

**Logs de um pod específico**

```bash
kubectl logs <pod-name> -n projecttemplate
```

**Logs anteriores (pod reiniciado)**

```bash
kubectl logs <pod-name> -n projecttemplate --previous
```

### Verificar Status dos Pods

**Listar pods**

```bash
kubectl get pods -n projecttemplate
```

**Detalhes de um pod**

```bash
kubectl describe pod <pod-name> -n projecttemplate
```

**Ver eventos**

```bash
kubectl get events -n projecttemplate --sort-by='.lastTimestamp'
```

### Executar Comandos no Container

```bash
kubectl exec -it <pod-name> -n projecttemplate -- /bin/bash
```

### Health Checks

**Port forward**

```bash
kubectl port-forward svc/projecttemplate-api 8080:80 -n projecttemplate
```

**Testar health checks**

```bash
curl http://localhost:8080/health
curl http://localhost:8080/health/ready
```

---

## Configurações Avançadas

### Horizontal Pod Autoscaler (HPA)

#### Linha de comando

```bash
kubectl autoscale deployment projecttemplate-api \
  --cpu-percent=70 \
  --min=2 \
  --max=10 \
  -n projecttemplate
```

#### Manifesto

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: projecttemplate-api
  namespace: projecttemplate
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: projecttemplate-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: 80
```

### Persistent Volume (para banco de dados)

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: database-pvc
  namespace: projecttemplate
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 10Gi
```

### TLS/HTTPS no Ingress

#### 1. Criar certificado (cert-manager)

```yaml
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: projecttemplate-tls
  namespace: projecttemplate
spec:
  secretName: projecttemplate-tls
  issuerRef:
    name: letsencrypt-prod
    kind: ClusterIssuer
  dnsNames:
    - api.projecttemplate.com
```

#### 2. Atualizar ingress

```yaml
spec:
  tls:
    - hosts:
        - api.projecttemplate.com
      secretName: projecttemplate-tls
```

---

## Ambientes (Dev, Staging, Prod)

### Usando Kustomize Overlays

**Estrutura:**

```text
.k8s/
├── base/
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── ingress.yaml
│   └── kustomization.yaml
└── overlays/
    ├── dev/
    │   ├── kustomization.yaml
    │   └── patches/
    ├── staging/
    │   ├── kustomization.yaml
    │   └── patches/
    └── production/
        ├── kustomization.yaml
        └── patches/
```

**Deploy por ambiente:**

```bash
# Development
kubectl apply -k .k8s/overlays/dev/

# Staging
kubectl apply -k .k8s/overlays/staging/

# Production
kubectl apply -k .k8s/overlays/production/
```

---

## Segurança

### Network Policies

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: api-network-policy
  namespace: projecttemplate
spec:
  podSelector:
    matchLabels:
      app: projecttemplate-api
  policyTypes:
    - Ingress
    - Egress
  ingress:
    - from:
        - podSelector:
            matchLabels:
              app: ingress-nginx
      ports:
        - protocol: TCP
          port: 8080
  egress:
    - to:
        - podSelector:
            matchLabels:
              app: database
      ports:
        - protocol: TCP
          port: 5432
```

### Pod Security Standards

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: projecttemplate
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted
```

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Deploy to Kubernetes

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Build Docker image
        run: |
          docker build -t ${{ secrets.REGISTRY }}/projecttemplate-api:${{ github.sha }} .
          docker push ${{ secrets.REGISTRY }}/projecttemplate-api:${{ github.sha }}

      - name: Deploy to Kubernetes
        uses: azure/k8s-deploy@v1
        with:
          manifests: |
            .k8s/deployment.yaml
            .k8s/service.yaml
          images: |
            ${{ secrets.REGISTRY }}/projecttemplate-api:${{ github.sha }}
```

---

## Recursos Úteis

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [kubectl Cheat Sheet](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)
- [Kustomize Documentation](https://kustomize.io/)
- [Minikube Documentation](https://minikube.sigs.k8s.io/docs/)

---

## Problemas Comuns

### Pod não inicia (ImagePullBackOff)

```bash
# Verificar eventos
kubectl describe pod <pod-name> -n projecttemplate

# Conferir imagens disponíveis
minikube image ls | grep projecttemplate
```

**Solução:** verifique se a imagem existe, está com a tag correta e se o cluster possui acesso ao registry.

### Pod reiniciando constantemente (CrashLoopBackOff)

```bash
kubectl logs <pod-name> -n projecttemplate
```

**Solução:** valide configurações, dependências externas e health checks.

### Não consegue acessar a aplicação

```bash
# Verificar service
kubectl get svc -n projecttemplate

# Verificar ingress
kubectl get ingress -n projecttemplate

# Testar dentro do cluster
kubectl run -it --rm debug --image=curlimages/curl --restart=Never -- \
  curl http://projecttemplate-api.projecttemplate.svc.cluster.local/health
```

---

Para mais informações, consulte a documentação oficial do Kubernetes ou abra uma issue no repositório.
