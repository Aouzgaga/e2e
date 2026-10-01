# e2e

Tests end-to-end (e2e) en .NET pour l'application de saisie de congés.

## Application testée

- URL : https://aouzgaga.github.io/formation-gh-api/

## Scénario

- Vérifier que l'application répond bien (code de statut HTTP 2xx).

## Exécuter les tests localement

Prérequis : [.NET SDK](https://dotnet.microsoft.com/download) (voir `global.json`/`TargetFramework` du projet de tests).

```bash
dotnet test
```

Les tests sont exécutés automatiquement via GitHub Actions (voir `.github/workflows/e2e-tests.yml`).
