using KitapyurduData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitapyurduData.Data.Models;

public class TransferInfo
{
    public int Id { get; set; }
    public int EnSonKaldigiSayfa { get; set; }
    public int EnSonKaldigiKategoriId { get; set; }
    public TransferDurum TransferDurum { get; set; }
}
