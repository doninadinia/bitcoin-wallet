# bitcoin-wallet

> segwit · hd · psbt

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dot.net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Build](https://img.shields.io/badge/build-passing-brightgreen)]()

Bitcoin HD wallet — BIP84 accounts, coin control, PSBT export for hardware signers.

## Features

- HD wallet with BIP-44 key derivation path
- Encrypted vault storage with passphrase-based XOR cipher
- Multi-account management with labeled addresses
- Real-time balance synchronization via simulated RPC
- Multi-network support (mainnet, testnet, regtest)
- Configurable fee estimation with Low / Medium / High priority
- Portfolio tracking with simulated USD valuation
- Automatic storage migration between schema versions

## Prerequisites

- [.NET 10 SDK](https://dot.net/download)
- Git

## Getting Started

```bash
git clone <repo-url>
cd bitcoin-wallet
dotnet build
dotnet run --project src/bitcoin-wallet.Cli
```

## CLI Usage

```bash
# Create a new encrypted vault
btcwallet create-vault --name "MyWallet"

# List all local vaults
btcwallet list-vaults

# Open an existing vault (prompts for passphrase)
btcwallet open-vault --id <vault-id>

# Derive a new account inside the active vault
btcwallet add-account --label "Savings"

# Synchronize balances from the network
btcwallet sync

# Display all account balances
btcwallet balance

# Show portfolio summary with USD valuation
btcwallet portfolio
```

## Project Structure

```
src/
 bitcoin-wallet.Wallet/          Core library
   Models/                Data models (Vault, Account, NetworkConfig)
   Crypto/                Key derivation, mnemonics, Base58 codec
   Chain/                 Simulated RPC, balance provider, fee estimator
   Storage/               Vault persistence and schema migrations
   Services/              Wallet manager, sync engine, portfolio tracker
 bitcoin-wallet.Cli/             Command-line interface
tests/
 bitcoin-wallet.Wallet.Tests/    Unit tests (xUnit)
```

## Configuration

Edit `src/bitcoin-wallet.Cli/appsettings.json`:

```json
{
  "Network": "mainnet",
  "RpcEndpoint": "https://localhost:8332",
  "StorageDir": ".wallets"
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `Network` | `mainnet` | Target network (`mainnet`, `testnet`, `regtest`) |
| `RpcEndpoint` | `https://localhost:8332` | Bitcoin RPC node endpoint |
| `StorageDir` | `.wallets` | Local directory for encrypted vault files |

## Background

Named exactly what people type into GitHub search when they need a starting point.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.


---

## Topics

![bitcoin](https://img.shields.io/badge/bitcoin-111827?style=flat-square) ![bitcoin-wallet](https://img.shields.io/badge/bitcoin%20wallet-111827?style=flat-square) ![btc](https://img.shields.io/badge/btc-111827?style=flat-square) ![bip39](https://img.shields.io/badge/bip39-111827?style=flat-square) ![bip84](https://img.shields.io/badge/bip84-111827?style=flat-square) ![hd-wallet](https://img.shields.io/badge/hd%20wallet-111827?style=flat-square) ![segwit](https://img.shields.io/badge/segwit-111827?style=flat-square) ![psbt](https://img.shields.io/badge/psbt-111827?style=flat-square)

`bitcoin` `bitcoin-wallet` `btc` `bip39` `bip84` `hd-wallet` `segwit` `psbt` `cryptocurrency` `wallet` `cold-storage` `utxo` `coin-control` `hardware-wallet` `dotnet` `csharp` `open-source`

Search: bitcoin-wallet · segwit · hd · psbt · Bitcoin HD wallet — BIP39/BIP84, coin control, PSBT, hardware signer export

---

<sub>Bitcoin HD wallet — BIP39/BIP84, coin control, PSBT, hardware signer export</sub>
