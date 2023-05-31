using System.Threading.Channels;
using HugeFileSorter.Sorting;
using HugeFileSorter.Tests.Helpers;

namespace HugeFileSorter.Tests;

public class MergeTests
{
    [Theory]
    [InlineData(@"1. Apple
415. Apple")]
    [InlineData(@"1. Apple
1. Banana")]
    [InlineData(@"1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
")]
    [InlineData(@"5907. Raspberry mulberry
5912. Raspberry mulberry
5912. Raspberry mulberry
5912. Raspberry mulberry
5912. Raspberry mulberry
5912. Raspberry mulberry
5912. Raspberry mulberry
5920. Raspberry mulberry
5945. Raspberry mulberry
5967. Raspberry mulberry
5987. Raspberry mulberry
5990. Raspberry mulberry
6029. Raspberry mulberry
6048. Raspberry mulberry
6062. Raspberry mulberry
6065. Raspberry mulberry
6066. Raspberry mulberry
6111. Raspberry mulberry
6134. Raspberry mulberry
6154. Raspberry mulberry
6157. Raspberry mulberry
6157. Raspberry mulberry
6165. Raspberry mulberry
6184. Raspberry mulberry
6187. Raspberry mulberry
6191. Raspberry mulberry
6215. Raspberry mulberry
6221. Raspberry mulberry
6258. Raspberry mulberry
6263. Raspberry mulberry
6263. Raspberry mulberry
6269. Raspberry mulberry
6287. Raspberry mulberry
6310. Raspberry mulberry
6325. Raspberry mulberry
6358. Raspberry mulberry
6366. Raspberry mulberry
6369. Raspberry mulberry
6389. Raspberry mulberry
6413. Raspberry mulberry
6429. Raspberry mulberry
6435. Raspberry mulberry
6446. Raspberry mulberry
6461. Raspberry mulberry
6463. Raspberry mulberry
6488. Raspberry mulberry
6526. Raspberry mulberry
6527. Raspberry mulberry
6560. Raspberry mulberry
6560. Raspberry mulberry
6561. Raspberry mulberry
6567. Raspberry mulberry
6573. Raspberry mulberry
6583. Raspberry mulberry
6592. Raspberry mulberry
6595. Raspberry mulberry
6598. Raspberry mulberry
6598. Raspberry mulberry
6605. Raspberry mulberry
6639. Raspberry mulberry
6647. Raspberry mulberry
6655. Raspberry mulberry
6660. Raspberry mulberry
6661. Raspberry mulberry
6667. Raspberry mulberry
6681. Raspberry mulberry
6689. Raspberry mulberry
6701. Raspberry mulberry
6702. Raspberry mulberry
6703. Raspberry mulberry
6718. Raspberry mulberry
6720. Raspberry mulberry
6752. Raspberry mulberry
6753. Raspberry mulberry
6789. Raspberry mulberry
6815. Raspberry mulberry
6843. Raspberry mulberry
6844. Raspberry mulberry
6852. Raspberry mulberry
6853. Raspberry mulberry
6881. Raspberry mulberry
6890. Raspberry mulberry
6896. Raspberry mulberry
6899. Raspberry mulberry
6906. Raspberry mulberry
6932. Raspberry mulberry
6934. Raspberry mulberry
6935. Raspberry mulberry
6956. Raspberry mulberry
6970. Raspberry mulberry
6984. Raspberry mulberry
6988. Raspberry mulberry
7000. Raspberry mulberry
7030. Raspberry mulberry
7039. Raspberry mulberry
7047. Raspberry mulberry
7095. Raspberry mulberry
7114. Raspberry mulberry
7122. Raspberry mulberry
7126. Raspberry mulberry
7149. Raspberry mulberry
7165. Raspberry mulberry
7167. Raspberry mulberry
7170. Raspberry mulberry
7180. Raspberry mulberry
7193. Raspberry mulberry
7193. Raspberry mulberry
7229. Raspberry mulberry
7257. Raspberry mulberry
7297. Raspberry mulberry
7303. Raspberry mulberry
7315. Raspberry mulberry
7354. Raspberry mulberry
7377. Raspberry mulberry
7379. Raspberry mulberry
7380. Raspberry mulberry
7384. Raspberry mulberry
7411. Raspberry mulberry
7414. Raspberry mulberry
7416. Raspberry mulberry
7423. Raspberry mulberry
7431. Raspberry mulberry
7449. Raspberry mulberry
7493. Raspberry mulberry
7499. Raspberry mulberry
7501. Raspberry mulberry
7509. Raspberry mulberry
7516. Raspberry mulberry
7533. Raspberry mulberry
7558. Raspberry mulberry
7565. Raspberry mulberry
7581. Raspberry mulberry
7619. Raspberry mulberry
7623. Raspberry mulberry
7629. Raspberry mulberry
7635. Raspberry mulberry
7647. Raspberry mulberry
7681. Raspberry mulberry
7694. Raspberry mulberry
7712. Raspberry mulberry
7719. Raspberry mulberry
7722. Raspberry mulberry
7736. Raspberry mulberry
7787. Raspberry mulberry
7806. Raspberry mulberry
7807. Raspberry mulberry
7814. Raspberry mulberry
7824. Raspberry mulberry
7828. Raspberry mulberry
7836. Raspberry mulberry
7844. Raspberry mulberry
7853. Raspberry mulberry
7854. Raspberry mulberry
7855. Raspberry mulberry
7858. Raspberry mulberry
7864. Raspberry mulberry
7884. Raspberry mulberry
7889. Raspberry mulberry
7900. Raspberry mulberry
7900. Raspberry mulberry
7904. Raspberry mulberry
7911. Raspberry mulberry
7914. Raspberry mulberry
7950. Raspberry mulberry
7957. Raspberry mulberry
7962. Raspberry mulberry
7984. Raspberry mulberry
7991. Raspberry mulberry
8003. Raspberry mulberry
8030. Raspberry mulberry
8046. Raspberry mulberry
8047. Raspberry mulberry
8087. Raspberry mulberry
8096. Raspberry mulberry
8108. Raspberry mulberry
8117. Raspberry mulberry
8127. Raspberry mulberry
8128. Raspberry mulberry
8153. Raspberry mulberry
8164. Raspberry mulberry
8178. Raspberry mulberry
8178. Raspberry mulberry
8183. Raspberry mulberry
8195. Raspberry mulberry
8198. Raspberry mulberry
8202. Raspberry mulberry
8213. Raspberry mulberry
8223. Raspberry mulberry
8229. Raspberry mulberry
8236. Raspberry mulberry
8244. Raspberry mulberry
8253. Raspberry mulberry
8281. Raspberry mulberry
8286. Raspberry mulberry
8287. Raspberry mulberry
8295. Raspberry mulberry
8318. Raspberry mulberry
8320. Raspberry mulberry
8322. Raspberry mulberry
8331. Raspberry mulberry
8332. Raspberry mulberry
8360. Raspberry mulberry
8362. Raspberry mulberry
8373. Raspberry mulberry
8376. Raspberry mulberry
8395. Raspberry mulberry
8418. Raspberry mulberry
8455. Raspberry mulberry
8460. Raspberry mulberry
8462. Raspberry mulberry
8467. Raspberry mulberry
8483. Raspberry mulberry
8493. Raspberry mulberry
8498. Raspberry mulberry
8500. Raspberry mulberry
8504. Raspberry mulberry
8507. Raspberry mulberry
8512. Raspberry mulberry
8517. Raspberry mulberry
8528. Raspberry mulberry
8529. Raspberry mulberry
8531. Raspberry mulberry
8537. Raspberry mulberry
8537. Raspberry mulberry
8553. Raspberry mulberry
8565. Raspberry mulberry
8569. Raspberry mulberry
8577. Raspberry mulberry
8578. Raspberry mulberry
8581. Raspberry mulberry
8585. Raspberry mulberry
8587. Raspberry mulberry
8592. Raspberry mulberry
8603. Raspberry mulberry
8617. Raspberry mulberry
8618. Raspberry mulberry
8643. Raspberry mulberry
8643. Raspberry mulberry
8655. Raspberry mulberry
8667. Raspberry mulberry
8680. Raspberry mulberry
8684. Raspberry mulberry
8700. Raspberry mulberry
8707. Raspberry mulberry
8714. Raspberry mulberry
8717. Raspberry mulberry
8719. Raspberry mulberry
8730. Raspberry mulberry
8748. Raspberry mulberry
8750. Raspberry mulberry
8763. Raspberry mulberry
8769. Raspberry mulberry
8771. Raspberry mulberry
8772. Raspberry mulberry
8781. Raspberry mulberry
8802. Raspberry mulberry
8822. Raspberry mulberry
8826. Raspberry mulberry
8874. Raspberry mulberry
8892. Raspberry mulberry
8896. Raspberry mulberry
8922. Raspberry mulberry
8923. Raspberry mulberry
8930. Raspberry mulberry
8946. Raspberry mulberry
8966. Raspberry mulberry
8968. Raspberry mulberry
8970. Raspberry mulberry
8971. Raspberry mulberry
8974. Raspberry mulberry
8979. Raspberry mulberry
8986. Raspberry mulberry
8992. Raspberry mulberry
8997. Raspberry mulberry
8997. Raspberry mulberry
8998. Raspberry mulberry
9042. Raspberry mulberry
9055. Raspberry mulberry
9108. Raspberry mulberry
9117. Raspberry mulberry
9133. Raspberry mulberry
9159. Raspberry mulberry
9168. Raspberry mulberry
9173. Raspberry mulberry
9173. Raspberry mulberry
9200. Raspberry mulberry
9241. Raspberry mulberry
9258. Raspberry mulberry
9264. Raspberry mulberry
9265. Raspberry mulberry
9266. Raspberry mulberry
9276. Raspberry mulberry
9276. Raspberry mulberry
9282. Raspberry mulberry
9300. Raspberry mulberry
9305. Raspberry mulberry
9319. Raspberry mulberry
9331. Raspberry mulberry
9334. Raspberry mulberry
9338. Raspberry mulberry
9351. Raspberry mulberry
9374. Raspberry mulberry
9393. Raspberry mulberry
9424. Raspberry mulberry
9428. Raspberry mulberry
9446. Raspberry mulberry
9451. Raspberry mulberry
9452. Raspberry mulberry
9454. Raspberry mulberry
9467. Raspberry mulberry
9475. Raspberry mulberry
9483. Raspberry mulberry
9504. Raspberry mulberry
9523. Raspberry mulberry
9529. Raspberry mulberry
9532. Raspberry mulberry
9538. Raspberry mulberry
9551. Raspberry mulberry
9559. Raspberry mulberry
9569. Raspberry mulberry
9584. Raspberry mulberry
9588. Raspberry mulberry
9589. Raspberry mulberry
9596. Raspberry mulberry
9602. Raspberry mulberry
9614. Raspberry mulberry
9614. Raspberry mulberry
9636. Raspberry mulberry
9638. Raspberry mulberry
9669. Raspberry mulberry
9692. Raspberry mulberry
9695. Raspberry mulberry
9699. Raspberry mulberry
9700. Raspberry mulberry
9740. Raspberry mulberry
9764. Raspberry mulberry
9779. Raspberry mulberry
9784. Raspberry mulberry
9788. Raspberry mulberry
9790. Raspberry mulberry
9795. Raspberry mulberry
9818. Raspberry mulberry
9891. Raspberry mulberry
9907. Raspberry mulberry
9924. Raspberry mulberry
9926. Raspberry mulberry
9928. Raspberry mulberry
9949. Raspberry mulberry
9955. Raspberry mulberry
9957. Raspberry mulberry
9997. Raspberry mulberry")]
    public async Task Merge_Test(string sortedText)
    {
        //given 
        var comparer = new RowComparer();
        var merge = new MergeStrategy(comparer);

        var channel = Channel.CreateBounded<Row>(new BoundedChannelOptions(capacity: 1)
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = true,
        });

        var sortedCollections = new List<IEnumerable<Row>>();

        var chunks = 100;
        var totalRows = 0;

        for (var i = 0; i < chunks; ++i)
        {
            var sortedRows = RowFactory.Parse(sortedText);
            var skip = new Random().Next(0, sortedRows.Length);
            sortedRows = sortedRows.Skip(skip).ToArray();
            totalRows += sortedRows.Length;
            sortedCollections.Add(sortedRows);
        }

        //when
        var mergeTask = Task.Run(() => merge.MergeAsync(channel.Writer, sortedCollections));
        var readTask = Consumer.ReadAll(channel.Reader);
        
        await Task.WhenAll(mergeTask, readTask);

        var actual = readTask.Result;
        
        //then
        Assert.Equal(totalRows, actual.Length);
        
        for(var i = 1; i < actual.Length; ++i)
        {
            var prev = actual[i - 1];
            var curr = actual[i];
            var prevText = prev.ToString();
            var currText = curr.ToString();
            var diff = String
                .Compare(prevText.Substring(prev.FirstCharIndex), currText.Substring(curr.FirstCharIndex), StringComparison.InvariantCulture);
            
            if (diff == 0)
                diff = prev.Number.CompareTo(curr.Number);
            
            Assert.True(diff <= 0, $"{prev.ToString()} compared with {curr.ToString()}");
        }
    }
}