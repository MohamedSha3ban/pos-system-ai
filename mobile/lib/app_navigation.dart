import 'package:flutter/material.dart';

/// Lets ApiClient redirect to the login screen from deep inside a failed HTTP call (when a
/// refresh attempt also fails) without needing a BuildContext passed all the way down
/// through every service method.
final GlobalKey<NavigatorState> appNavigatorKey = GlobalKey<NavigatorState>();
