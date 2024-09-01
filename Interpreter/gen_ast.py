from pathlib import Path
from typing import TextIO

TAB = "    "


def define_ast(out_dir: Path, base_name: str, types: list[str]) -> None:
    file = out_dir.joinpath(f"{base_name}.cs")
    with file.open("w") as f:
        f.write("namespace Interpreter;\n\n")
        # Abstract class definition
        f.write(f"public abstract class {base_name} {{\n")
        # Visitor interface
        define_visitor(f, base_name, types, 1)
        f.write("\n")
        # Abstract accept method
        f.write(f"{TAB}public abstract T accept<T>(Visitor<T> visitor);\n")
        for type in types:
            f.write("\n")
            type_args = type.split(":")
            type_name = type_args[0].strip()
            type_fields = type_args[1].strip()
            # Subclass definitions
            define_type(f, base_name, type_name, type_fields, 1)
        # Closing bracket
        f.write("}\n")


def define_visitor(f: TextIO, base: str, types: list[str], tab_level: int) -> None:
    # Interface definition
    f.write(f"{TAB * tab_level}public interface Visitor<T> {{\n")
    for type in types:
        type_name = type.split(":")[0].strip()
        # Required methods
        f.write(
            f"{TAB * (tab_level + 1)}T visit{type_name}{base}({type_name} {base.lower()});\n"
        )
    # Closing bracket
    f.write(f"{TAB * tab_level}}}\n")


def define_type(f: TextIO, base: str, type: str, fields: str, tab_level: int) -> None:
    # Class definition
    f.write(f"{TAB * tab_level}public class {type} : {base} {{\n")
    field_list = [x.strip() for x in fields.split(",")]
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
    # Overwrite accept method
    f.write(
        f"{TAB * (tab_level + 1)}public override T accept<T>(Visitor<T> visitor) {{\n"
    )
    f.write(f"{TAB * (tab_level + 2)}return visitor.visit{type}{base}(this);\n")
    f.write(f"{TAB * (tab_level + 1)}}}\n")
    # Closing bracket
    f.write(f"{TAB * tab_level}}}\n")


if __name__ == "__main__":
    out_dir = Path(".")
    base_name = "Expr"
    types = [
        "Binary   : Expr left, Token op, Expr right",
        "Grouping : Expr expression",
        "Literal  : Object? value",
        "Unary    : Token op, Expr right",
    ]
    define_ast(out_dir, base_name, types)
