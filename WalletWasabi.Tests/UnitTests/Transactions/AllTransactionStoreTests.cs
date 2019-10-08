using NBitcoin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WalletWasabi.Models;
using WalletWasabi.Transactions;
using Xunit;

namespace WalletWasabi.Tests.UnitTests.Transactions
{
	public class AllTransactionStoreTests
	{
		[Theory]
		[MemberData(nameof(GetDifferentNetworkValues))]
		public async Task CanInitializeAsync(Network network)
		{
			var txStore = new AllTransactionStore();
			await txStore.InitializeAsync(PrepareWorkDir(), network, ensureBackwardsCompatibility: false);

			Assert.NotNull(txStore.ConfirmedStore);
			Assert.NotNull(txStore.MempoolStore);
			Assert.Empty(txStore.GetTransactions());
			Assert.Empty(txStore.GetTransactionHashes());
			Assert.Empty(txStore.MempoolStore.GetTransactions());
			Assert.Empty(txStore.MempoolStore.GetTransactionHashes());
			Assert.Empty(txStore.ConfirmedStore.GetTransactions());
			Assert.Empty(txStore.ConfirmedStore.GetTransactionHashes());

			uint256 txHash = Global.GenerateRandomSmartTransaction().GetHash();
			Assert.False(txStore.Contains(txHash));
			Assert.True(txStore.IsEmpty());
			Assert.False(txStore.TryGetTransaction(txHash, out _));

			var dir = GetWorkDir();
			var mempoolFile = Path.Combine(dir, "Mempool", "Transactions.dat");
			var txFile = Path.Combine(dir, "ConfirmedTransactions", "Transactions.dat");
			var mempoolContent = await File.ReadAllBytesAsync(mempoolFile);
			var txContent = await File.ReadAllBytesAsync(txFile);

			Assert.True(File.Exists(mempoolFile));
			Assert.True(File.Exists(txFile));
			Assert.Empty(mempoolContent);
			Assert.Empty(txContent);
		}

		[Theory]
		[MemberData(nameof(GetDifferentNetworkValues))]
		public async Task DoesntUpdateAsync(Network network)
		{
			var txStore = new AllTransactionStore();
			await txStore.InitializeAsync(PrepareWorkDir(), network, ensureBackwardsCompatibility: false);

			var tx = Global.GenerateRandomSmartTransaction();
			Assert.False(txStore.TryUpdate(tx, out _));

			// Assert TryUpdate didn't modify anything.

			Assert.NotNull(txStore.ConfirmedStore);
			Assert.NotNull(txStore.MempoolStore);
			Assert.Empty(txStore.GetTransactions());
			Assert.Empty(txStore.GetTransactionHashes());
			Assert.Empty(txStore.MempoolStore.GetTransactions());
			Assert.Empty(txStore.MempoolStore.GetTransactionHashes());
			Assert.Empty(txStore.ConfirmedStore.GetTransactions());
			Assert.Empty(txStore.ConfirmedStore.GetTransactionHashes());

			uint256 txHash = Global.GenerateRandomSmartTransaction().GetHash();
			Assert.False(txStore.Contains(txHash));
			Assert.True(txStore.IsEmpty());
			Assert.False(txStore.TryGetTransaction(txHash, out _));
		}

		#region Helpers

		private string PrepareWorkDir([CallerMemberName] string caller = null)
		{
			string dir = GetWorkDir(caller);
			if (Directory.Exists(dir))
			{
				Directory.Delete(dir, true);
			}

			return dir;
		}

		private static string GetWorkDir([CallerMemberName] string caller = null)
		{
			// Make sure starts with clear state.
			return Path.Combine(Global.Instance.DataDir, nameof(AllTransactionStoreTests), caller);
		}

		public static IEnumerable<object[]> GetDifferentNetworkValues()
		{
			var networks = new List<Network>
			{
				Network.Main,
				Network.TestNet,
				Network.RegTest
			};

			foreach (Network network in networks)
			{
				yield return new object[] { network };
			}
		}

		#endregion Helpers
	}
}
