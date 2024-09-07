from pathlib import Path
from typing import TextIO

TAB = "    "


def define_ast(
    out_dir: Path, base_name: str, types: list[str], is_generic: bool
) -> None:
    file = out_dir.joinpath(f"{base_name}.cs")
    with file.open("w") as f:
        f.write("namespace Lib;\n\n")
        # Abstract class definition
        f.write(f"public abstract class {base_name} {{\n")
        # IVisitor interface
        generic = f"<T>" if is_generic else ""
        return_type = "T" if is_generic else "void"
        define_visitor(f, base_name, types, 1, generic, return_type)
        f.write("\n")
        # Abstract Accept method
        f.write(
            f"{TAB}public abstract {return_type} Accept{generic}(IVisitor{generic} visitor);\n"
        )
        for type in types:
            f.write("\n")
            type_args = type.split(":")
            type_name = type_args[0].strip()
            type_fields = type_args[1].strip()
            # Subclass definitions
            define_type(f, base_name, type_name, type_fields, 1, generic, return_type)
        # Closing bracket
        f.write("}\n")


def define_visitor(
    f: TextIO,
    base: str,
    types: list[str],
    tab_level: int,
    generic: str,
    return_type: str,
) -> None:
    # Interface definition
    f.write(f"{TAB * tab_level}public interface IVisitor{generic} {{\n")
    for type in types:
        type_name = type.split(":")[0].strip()
        # Required methods
        f.write(
            f"{TAB * (tab_level + 1)}{return_type} Visit{type_name}{base}({type_name} {base.lower()});\n"
        )
    # Closing bracket
    f.write(f"{TAB * tab_level}}}\n")


def define_type(
    f: TextIO,
    base: str,
    type: str,
    fields: str,
    tab_level: int,
    generic: str,
    return_type: str,
) -> None:
    # Class definition
    f.write(f"{TAB * tab_level}public class {type} : {base} {{\n")
    field_list = [x.strip() for x in fields.split(",") if x != ""]
    # Field names
    for field in field_list:
        f.write(f"{TAB * (tab_level + 1)}public readonly {field.replace(" ", " _")};\n")
    f.write("\n")
    # Constructor
    f.write(f"{TAB * (tab_level + 1)}public {type}({fields}) {{\n")
    for field in field_list:
        name = field.split(" ")[1]
        f.write(f"{TAB * (tab_level + 2)}_{name} = {name};\n")
    f.write(f"{TAB * (tab_level + 1)}}}\n")
    f.write("\n")
    # Overwrite Accept method
    f.write(
        f"{TAB * (tab_level + 1)}public override {return_type} Accept{generic}(IVisitor{generic} visitor) {{\n"
    )
    return_token = "" if return_type == "void" else "return "
    f.write(f"{TAB * (tab_level + 2)}{return_token}visitor.Visit{type}{base}(this);\n")
    f.write(f"{TAB * (tab_level + 1)}}}\n")
    # Closing bracket
    f.write(f"{TAB * tab_level}}}\n")


if __name__ == "__main__":
    out_dir = Path(".")
    base_name = "Expr"
    types = [
        "Assign   : Token name, Expr value",
        "Binary   : Expr left, Token op, Expr right",
        "Call     : Expr callee, Token paren, List<Expr> args",
        "Empty    :",
        "Get      : Expr instance, Token name",
        "Grouping : Expr expression",
        "Literal  : object? value",
        "Logical  : Expr left, Token op, Expr right",
        "Set      : Expr instance, Token name, Expr value",
        "This     : Token keyword",
        "Unary    : Token op, Expr right",
        "Variable : Token name",
    ]
    define_ast(out_dir, base_name, types, True)
    base_name = "Stmt"
    types = [
        "Block      : List<Stmt> statements",
        "Class      : Token name, List<Stmt.Function> methods",
        "Expression : Expr expression",
        "Function   : Token name, List<Token> parameters, List<Stmt> body",
        "If         : Expr condition, Stmt thenBranch, Stmt? elseBranch",
        "Return     : Token keyword, Expr? value",
        "Var        : Token name, Expr? initializer",
        "While      : Expr condition, Stmt body",
    ]
    define_ast(out_dir, base_name, types, False)
