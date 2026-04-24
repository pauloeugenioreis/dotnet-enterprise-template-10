import 'package:flutter/material.dart';

/// Design System do Enterprise Template.
/// Paleta: primary-600 (#0284c7) — consistente com BlazorWebApp e MauiApp.
class AppTheme {
  AppTheme._();

  // ─── Brand Colors ────────────────────────────────────────────────
  static const Color primary50  = Color(0xFFF0F9FF);
  static const Color primary100 = Color(0xFFE0F2FE);
  static const Color primary200 = Color(0xFFBAE6FD);
  static const Color primary400 = Color(0xFF38BDF8);
  static const Color primary600 = Color(0xFF0284C7); // Brand principal
  static const Color primary700 = Color(0xFF0369A1);
  static const Color primary800 = Color(0xFF075985);
  static const Color primary900 = Color(0xFF0C4A6E);

  // ─── Neutral ─────────────────────────────────────────────────────
  static const Color gray50  = Color(0xFFF8FAFC);
  static const Color gray100 = Color(0xFFF1F5F9);
  static const Color gray200 = Color(0xFFE2E8F0);
  static const Color gray400 = Color(0xFF94A3B8);
  static const Color gray500 = Color(0xFF64748B);
  static const Color gray700 = Color(0xFF334155);
  static const Color gray900 = Color(0xFF0F172A);

  // ─── Semantic ─────────────────────────────────────────────────────
  static const Color success = Color(0xFF10B981);
  static const Color warning = Color(0xFFF59E0B);
  static const Color error   = Color(0xFFE11D48);

  // ─── Theme Data ───────────────────────────────────────────────────
  static ThemeData get lightTheme => ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.fromSeed(
      seedColor: primary600,
      primary: primary600,
      surface: Colors.white,
    ),
    fontFamily: 'Roboto', 
    scaffoldBackgroundColor: gray50,
    appBarTheme: const AppBarTheme(
      backgroundColor: primary600,
      foregroundColor: Colors.white,
      elevation: 0,
      centerTitle: false,
      titleTextStyle: TextStyle(
        fontSize: 18,
        fontWeight: FontWeight.bold,
        color: Colors.white,
      ),
    ),
    cardTheme: CardThemeData(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      color: Colors.white,
      surfaceTintColor: Colors.transparent,
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: primary600,
        foregroundColor: Colors.white,
        minimumSize: const Size(double.infinity, 56),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        textStyle: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
        elevation: 4,
        shadowColor: primary200,
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: const BorderSide(color: Color(0xFFE2E8F0)),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: const BorderSide(color: Color(0xFFE2E8F0)),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: const BorderSide(color: primary600, width: 2),
      ),
      contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
    ),
  );

  static InputDecoration inputDecoration(String label, {IconData? icon}) {
    return InputDecoration(
      labelText: label,
      floatingLabelBehavior: FloatingLabelBehavior.always,
      prefixIcon: icon != null ? Icon(icon, color: primary600) : null,
      hintStyle: const TextStyle(color: gray400, fontSize: 14),
      labelStyle: const TextStyle(color: primary600, fontSize: 13, fontWeight: FontWeight.w900),
      alignLabelWithHint: true,
    );
  }
}
