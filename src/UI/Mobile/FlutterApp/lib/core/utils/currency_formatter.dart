import 'package:intl/intl.dart';

class CurrencyFormatter {
  static final _brFormat = NumberFormat.currency(
    locale: 'pt_BR',
    symbol: 'R\$',
    decimalDigits: 2,
  );

  static String format(double value) {
    return _brFormat.format(value);
  }
}
